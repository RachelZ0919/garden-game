using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using System;
using System.Text.RegularExpressions;
using System.IO;

public class LevelBuilder : EditorWindow
{
    private int toolSetIndex = 0;

    private int levelSetID;

    #region ToolSet1

    #region General
    private int selectingTask;
    private PuzzleBlock selectingBlock;
    private Direction selectingDirection;

    private GameObject levelPrefab;
    private string defaultLevelName = "NewLevel-";
    private GameObject blockPrefab;
    private string blockName = "Block";
    private int blockCount = 0;

    
    private List<Transform> levelParents;
    private List<String> levelNames;
    private List<int> levelIndices;
    private int selectingParentIndex = 0;

    private string debugInfo;
    #endregion

    #region Task - Connect Grid

    private LevelGrid firstGrid;
    private Vector3[] firstGridHighlightPoints;
    private bool isSelectingNearbyGrid;
    private Direction relativeDirectionToFirstGrid;
    private bool connection;

    #endregion
   
    #region Task - Add Start
    private int selectingStartType;
    private string[] startTypeNames;
    #endregion

    #region Task - Add End
    private int selectingEndType;
    private string[] endTypeNames;
    #endregion

    #region - Add Fake Connection

    private Camera sceneCamera;
    private Camera playCamera;
    private Vector2 scrollpos = Vector2.zero;
    private int processNum = 0;
    
    private float eventAngle;
    
    private LevelGrid fcFirstGrid;
    private LevelGrid fcSecondGrid;

    private bool isSelectingDireciton = false;
    private Direction fcSelectingDirection;
    private Direction fcFirstDirection;
    private Direction fcSecondDirection;
    

    #endregion

    #region Visualization

    private Vector3[] highLightPoints;

    #endregion

    #endregion

    #region ToolSet2

    private LevelSets levelSets;
    private string dataPath = "\\Assets\\Editor\\Data\\LevelSetsData.txt";

    private List<string> levelSetNames;
    private List<int> levelSetIndices;

    private int editingLevelSetIndex = 0;
    private string defaltLevelSetName = "LevelSet";
    private int selectingLevelSetIndex = 0;

    private string[] notSprayablePath;
    private string[] framePath;
    private string[] planePath;
    private string[] startBasePath;
    private string[] endBasePath;
    private string[] startPath;
    private string[] endPath;
    private string prefabPath = "\\Assets\\Resources\\Prefabs\\Level Elements\\";

    #endregion



    [MenuItem("Tools/LevelBuilder")]
    public static void ShowWindow()
    {
        EditorWindow editorWindow = EditorWindow.GetWindow(typeof(LevelBuilder), true);
        editorWindow.autoRepaintOnSceneChange = true;
    }

    private void Update()
    {
        UpdateLevels();
    }

    private void OnSceneGUI(SceneView sceneview)
    {
        UpdateDebugInfo();
        Repaint();
        if (selectingTask < 6)
        {
            if (Event.current.alt || (Event.current.type == EventType.MouseDown || Event.current.type == EventType.MouseUp) && Event.current.button == 2)//Ignore View Changing
            {
                return;
            }
            if (Event.current.type == EventType.MouseMove)
            {
                if(selectingTask == 5)//if in task 5 and button not pressed, just skip
                {
                    if(processNum == 2 || processNum == 4)//2 means select the first grid, 4 means select the second grid
                    {
                        UpdateSelectingBlock();
                    }
                    else // have to select the direction
                    {
                        //todo : select direction
                        // Shoot a ray from the mouse position into the world
                        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
                        RaycastHit hit;
                        // Shoot this ray. check in a distance of 10000.
                        if (Physics.Raycast(worldRay, out hit, 10000))
                        {
                            //check if is the same block
                            if (hit.collider == fcFirstGrid.block.GetComponent<Collider>())
                            {
                                //check if is the same grid
                                if (fcFirstGrid.block.GetDirectionOfHitPoint(hit.point) == fcFirstGrid.direction)
                                {
                                    fcFirstGrid.block.GetConnectingDirectionOfHitPoint(hit.point);
                                }
                                isSelectingDireciton = true;
                                Debug.Log("Choosing Direction");
                            }
                            else
                            {
                                isSelectingDireciton = false;
                            }
                        }
                        else
                        {
                            isSelectingDireciton = false;
                        }
                    }
                }
                else
                {
                    UpdateSelectingBlock();
                    if (selectingTask == 1)
                    {
                        if (firstGrid != null && selectingBlock != null)
                        {
                            LevelGrid tmp = selectingBlock.GetGrid(selectingDirection);
                            if (tmp.isSprayable())
                            {
                                for (int i = 1; i <= 4; i++)
                                {
                                    if (tmp.GetNearGrid((Direction)i) == firstGrid)
                                    {
                                        isSelectingNearbyGrid = true;
                                        relativeDirectionToFirstGrid = (Direction)i;
                                        return;
                                    }
                                }
                            }
                            isSelectingNearbyGrid = false;
                        }
                    }
                }
            }
            else if (Event.current.type == EventType.MouseDown)
            {
                if(selectingTask == 0)
                {
                    Selection.activeObject = null;
                    if (selectingBlock != null)
                    {
                        if (Event.current.button == 0)
                        {
                            //Add a block
                            Transform parent = levelParents[selectingParentIndex];
                            Vector3 deltaPos = selectingBlock.GetDirection(selectingDirection);
                            Vector3 position = selectingBlock.transform.position + deltaPos;

                            PuzzleBlock newBlock = Instantiate(blockPrefab, position, parent.rotation, parent.Find("Blocks")).GetComponent<PuzzleBlock>();
                            newBlock.name = $"{blockName}{blockCount++}";
                            LevelSet set = levelSets.levelSets[selectingLevelSetIndex];
                            newBlock.InitializeGridVisual(set.notSprayablePath, set.framePath, set.planePath);
                            //Need to raycast so active lator
                            newBlock.gameObject.GetComponent<Collider>().enabled = false;
                            RaycastInDirectionAndAddBlock(Vector3.up, position, newBlock);
                            RaycastInDirectionAndAddBlock(Vector3.down, position, newBlock);
                            RaycastInDirectionAndAddBlock(Vector3.left, position, newBlock);
                            RaycastInDirectionAndAddBlock(Vector3.right, position, newBlock);
                            RaycastInDirectionAndAddBlock(Vector3.forward, position, newBlock);
                            RaycastInDirectionAndAddBlock(Vector3.back, position, newBlock);
                            newBlock.gameObject.GetComponent<Collider>().enabled = true;
                        }
                        else if (Event.current.button == 1)
                        { 
                            selectingBlock.ClearConnection();
                            Undo.DestroyObjectImmediate(selectingBlock.gameObject);
                        }
                    }
                    UpdateSelectingBlock();
                }
                else if(selectingTask == 1)
                {
                    if(Event.current.button == 0)//left mouse
                    {
                        if (firstGrid == null)
                        {
                            firstGrid = selectingBlock.GetGrid(selectingDirection);
                            if (!firstGrid.isSprayable())
                            {
                                firstGrid = null;
                            }
                            else
                            {
                                firstGridHighlightPoints = highLightPoints;
                                connection = true;
                            }
                        }
                        else
                        {
                            if (isSelectingNearbyGrid)
                            {
                                if (connection)
                                {
                                    selectingBlock.ConnectGrid(selectingDirection, relativeDirectionToFirstGrid);
                                }
                                else
                                {
                                    selectingBlock.DisconnectGrid(selectingDirection, relativeDirectionToFirstGrid);
                                }
                                
                                firstGrid = null;
                                isSelectingNearbyGrid = false;
                            }
                        }
                    }else if(Event.current.button == 1)
                    {
                        if(firstGrid != null)
                        {
                            firstGrid = null;
                            isSelectingNearbyGrid = false;
                        }
                        else
                        {
                            firstGrid = selectingBlock.GetGrid(selectingDirection);
                            if (!firstGrid.isSprayable())
                            {
                                firstGrid = null;
                            }
                            else
                            {
                                firstGridHighlightPoints = highLightPoints;
                                connection = false;
                            }
                        }
                    }   
                }
                else if(selectingTask == 2)
                {
                    LevelGrid grid = selectingBlock.GetGrid(selectingDirection);
                    selectingBlock.SetSprayable(!grid.isSprayable(),selectingDirection);
                }
                else if(selectingTask == 3)
                {
                    if(Event.current.button == 0) // left mouse
                    {
                        if(levelSets.levelSets[selectingLevelSetIndex].startSets.Count > 0)
                        {
                            string objPath = levelSets.levelSets[selectingLevelSetIndex].startSets[selectingStartType].objPath;
                            string basePath = levelSets.levelSets[selectingLevelSetIndex].startSets[selectingStartType].basePath;
                            if (!selectingBlock.SetStartPoint(objPath, basePath, selectingDirection))
                            {
                                Debug.Log($"{levelSets.levelSets[selectingLevelSetIndex].startSets[selectingStartType].objType} is not allowed to spawn at the direction {selectingDirection}");
                            }
                        }
 
                    }
                }
                else if(selectingTask == 4)
                {
                    if(Event.current.button == 0)//left mouse
                    {
                        if(levelSets.levelSets[selectingLevelSetIndex].endSets.Count > 0)
                        {
                            string objPath = levelSets.levelSets[selectingLevelSetIndex].endSets[selectingEndType].objPath;
                            string basePath = levelSets.levelSets[selectingLevelSetIndex].endSets[selectingEndType].basePath;
                            if (!selectingBlock.SetEndPoint(objPath, basePath, selectingDirection))
                            {
                                Debug.Log($"{levelSets.levelSets[selectingLevelSetIndex].endSets[selectingEndType].objType} is not allowed to spawn at the direction {selectingDirection}");
                            }
                        }
                    }
                }
                else if(selectingTask == 5)
                {
                    if (processNum == 2)//when click the first grid
                    {
                        fcFirstGrid = selectingBlock.GetGrid(selectingDirection);
                        processNum++;
                    }else if(processNum == 3)//when click the first direction
                    {
                        fcFirstDirection = fcSelectingDirection;
                        processNum++;
                    }else if(processNum == 4)//when click the second grid
                    {
                        LevelGrid grid = selectingBlock.GetGrid(selectingDirection);
                        if(grid != fcFirstGrid)
                        {
                            fcSecondGrid = selectingBlock.GetGrid(selectingDirection);
                            processNum++;
                        }
                    }
                    else if(processNum == 5)
                    {
                        fcSecondDirection = fcSelectingDirection;
                        //TODO:Set The Fake Connection
                        processNum = 0;
                    }
                }
                Event.current.Use();
            }
            else if (Event.current.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(GUIUtility.GetControlID(GetHashCode(), FocusType.Keyboard));
            }

            //Change View
            if(selectingTask != 1)//show the selected grid 
            {
                if (selectingBlock != null)
                {
                    if (selectingBlock.GetGrid(selectingDirection).isSprayable())
                    {
                        PaintSquare(highLightPoints, Color.yellow);
                    }
                    else
                    {
                        PaintSquare(highLightPoints, Color.grey);
                    }

                    LevelGrid grid = selectingBlock.GetGrid(selectingDirection);
                    Color[] colors = new[] { Color.red, Color.blue, Color.green, Color.cyan };
                    for(int i = 1;i <= 4; i++)
                    {
                        LevelGrid tmp = grid.GetNearGrid((Direction)i);
                        if(tmp != null)
                        {
                            PaintSquare(tmp.block.GetPointsForView(tmp.direction), Color.cyan);
                        }
                    }

                }
            }
            else
            {
                if(firstGrid != null)
                {
                    PaintSquare(firstGridHighlightPoints, Color.yellow);
                    if (isSelectingNearbyGrid)
                    {
                        PaintSquare(highLightPoints, Color.red);
                    }
                }
                else
                {
                    if(selectingBlock != null)
                    {
                        if (selectingBlock.GetGrid(selectingDirection).isSprayable())
                        {
                            PaintSquare(highLightPoints, Color.yellow);
                        }
                        else
                        {
                            PaintSquare(highLightPoints, Color.grey);
                        }
                    }
                }
            }

        }
    }

    void OnGUI()
    {
        #region GUI - Header
        GUILayout.Label("Level Builder");
        
        GUILayout.Space(10);

        toolSetIndex = GUILayout.Toolbar(toolSetIndex, new[] { "Build Level", "Level Sets Management" });
        
        GUILayout.Space(10);

        #endregion

        #region GUI - Toolset 1 
        if (toolSetIndex == 0)
        {
            #region Toolset1 - Level Management
            if (levelParents.Count == 0)
            {
                if(GUILayout.Button("Add A Level"))
                {
                    AddNewLevel();
                }
            }
            else
            {
                

                GUILayout.BeginVertical("Box");
                GUILayout.Label("Level Management");

                //select level
                int parentIndex = EditorGUILayout.IntPopup("Selecting Level", selectingParentIndex, levelNames.ToArray(), levelIndices.ToArray());
                UpdateSelectingLevelInfo(parentIndex);

                //level name
                string name = EditorGUILayout.TextField("Level Name", levelNames[selectingParentIndex]);
                ChangeLevelName(selectingParentIndex, name);

                //select level sets
                if(levelSets.levelSets.Count == 0)
                {
                    GUILayout.BeginVertical("box");
                    GUILayout.Label("Please First Add A Level Set To Start Building A Level!!!!");
                    GUILayout.EndVertical();
                    return;
                }
                else
                {
                    int levelSetIndex = EditorGUILayout.IntPopup("Using Level Set", selectingLevelSetIndex, levelSetNames.ToArray(), levelSetIndices.ToArray());
                    ChangeLevelSet(levelSetIndex);
                }

                GUILayout.Space(10);
                //add level
                if (GUILayout.Button("Add A Level"))
                {
                    AddNewLevel();
                }
                if (GUILayout.Button("Clear Out"))
                {
                    int count = levelParents[selectingParentIndex].Find("Blocks").childCount;
                    for (int i = 0; i < count; i++)
                    {
                        DestroyImmediate(levelParents[selectingParentIndex].Find("Blocks").GetChild(0).gameObject);
                    }
                }

                

            #endregion

            #region Toolset1 - Level Edit
                //level edit
                if (levelParents[selectingParentIndex].Find("Blocks").childCount == 0)
                {
                    if(GUILayout.Button("Start Build"))
                    {
                        PuzzleBlock newblock = Instantiate(blockPrefab, levelParents[selectingParentIndex].position, levelParents[selectingParentIndex].rotation, levelParents[selectingParentIndex].Find("Blocks")).GetComponent<PuzzleBlock>();
                        newblock.name = $"{blockName}{blockCount++}";
                        LevelSet set = levelSets.levelSets[selectingLevelSetIndex];
                        newblock.InitializeGridVisual(set.notSprayablePath, set.framePath, set.planePath);
                    }
                    GUILayout.Space(10);
                    GUILayout.EndVertical();
                }
                else
                {
                    GUILayout.EndVertical();
                    GUILayout.BeginHorizontal();

                    GUILayout.BeginVertical("Box");
                    GUILayout.Label("Select Task");
                    selectingTask = GUILayout.SelectionGrid(selectingTask, new[] {  "Add/Delete Blocks", 
                                                                                    "Connect Blocks", 
                                                                                    "Set Sprayable", 
                                                                                    "Set Start", 
                                                                                    "Set End",
                                                                                    "Set Fake Connection",
                                                                                    "Do nothing" }, 1,GUILayout.MinWidth(180));
                    GUILayout.EndVertical();
                    
                    GUILayout.BeginVertical();

                    GUILayout.BeginVertical("box", GUILayout.MinHeight(82));
                    GUILayout.Label("Chosing Start Object");
                    selectingStartType = GUILayout.SelectionGrid(selectingStartType, startTypeNames, 2);
                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("box", GUILayout.MinHeight(82));
                    GUILayout.Label("Chosing End Object");
                    selectingEndType = GUILayout.SelectionGrid(selectingEndType, endTypeNames, 2);
                    GUILayout.EndVertical();

                    GUILayout.EndVertical();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginVertical("box");
                    GUILayout.Label("Tool Box");

                    if (selectingTask == 5)
                    {
                        //Show all the fake connection
                        Level level = levelParents[selectingParentIndex].GetComponent<Level>();
                        for (int i = 0; i < level.fakeConnectionGridsList.Count; i++)
                        {
                            GUILayout.BeginHorizontal();

                            GUILayout.Label($"{level.fakeConnectionGridsList[i].grid1.name}");
                            GUILayout.Label($"{level.fakeConnectionGridsList[i].grid2.name}");
                            if (GUILayout.Button("X"))
                            {
                                //TODO:Tell To Delete The Connection
                            }

                            GUILayout.EndHorizontal();
                        }



                        //Add Connection Button
                        if (GUILayout.Button("Add One Connection") && processNum == 0)
                        {   
                            processNum = 1;
                        }

                        if(processNum == 1)
                        {
                            ObjectRotate rotate = level.GetComponent<ObjectRotate>();
                            scrollpos = GUILayout.BeginScrollView(scrollpos, false, true, GUILayout.Height(46));
                            for(int i = 0; i < rotate.eventAngles.Count; i++)
                            {
                                if (GUILayout.Button($"{rotate.eventAngles[i]}"))
                                {
                                    ChangeViewToTargetRotation(rotate, i);
                                    break;
                                }
                            }
                            GUILayout.EndScrollView();
                            if (GUILayout.Button("Confirm"))
                            {
                                processNum = 2;
                            }
                        }
                    }

                    GUILayout.EndVertical();

                    GUILayout.BeginVertical("box");
                    GUILayout.Label("Block Info");
                    GUILayout.Label(debugInfo);
                    GUILayout.EndVertical();
                }
                #endregion
            }
        }
        #endregion

        #region GUI - Toolset 2
        else if (toolSetIndex == 1)
        {
            if(levelSetNames.Count == 0)
            {
                GUILayout.BeginVertical("box");
                GUILayout.Label("There is no level set. Please create one first!");
                GUILayout.EndVertical();

                if(GUILayout.Button("Create A Level Set"))
                {
                    CreateALevelSet();
                }
            }
            else
            {
                //Select Level Set
                editingLevelSetIndex = EditorGUILayout.IntPopup("Level Set", editingLevelSetIndex, levelSetNames.ToArray(), levelSetIndices.ToArray());

                //ID
                GUILayout.Label($"ID:{levelSets.levelSets[editingLevelSetIndex].ID}");

                //Level Set Name
                string levelSetName = EditorGUILayout.TextField("Level Set Name", levelSets.levelSets[editingLevelSetIndex].setName);
                ChangeSetName(editingLevelSetIndex, levelSetName);

                //not sprayable path
                int notSprayableIndex = EditorGUILayout.Popup("NotSprayable Path", GetPathIndex(notSprayablePath, "NotSprayable", levelSets.levelSets[editingLevelSetIndex].notSprayablePath), notSprayablePath);
                levelSets.levelSets[editingLevelSetIndex].notSprayablePath = GetPathOfIndex(notSprayableIndex, notSprayablePath, "Prefabs/Level Elements/NotSprayable/");

                //frame path
                int frameIndex = EditorGUILayout.Popup("Frame Path", GetPathIndex(framePath, "Frame", levelSets.levelSets[editingLevelSetIndex].framePath), framePath);
                levelSets.levelSets[editingLevelSetIndex].framePath = GetPathOfIndex(frameIndex, framePath, "Prefabs/Level Elements/Frame/");

                //plane path
                int planeIndex = EditorGUILayout.Popup("Plane Path", GetPathIndex(planePath, "Normal", levelSets.levelSets[editingLevelSetIndex].planePath), planePath);
                levelSets.levelSets[editingLevelSetIndex].planePath = GetPathOfIndex(planeIndex, planePath, "Prefabs/Level Elements/Plane/Normal/");

                //start
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Label("Start Sets",GUILayout.MaxWidth(145));

                if (GUILayout.Button("Add Start Set"))
                {
                    LevelObjectSet objSet = new LevelObjectSet();
                    levelSets.levelSets[editingLevelSetIndex].startSets.Add(objSet);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Index",GUILayout.Width(50));
                GUILayout.Label("Start Type");
                GUILayout.Label("Object Path");
                GUILayout.Label("Base Path");
                GUILayout.Label(" ", GUILayout.Width(30));
                GUILayout.EndHorizontal();

                for (int i = 0; i < levelSets.levelSets[editingLevelSetIndex].startSets.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Set {i}", GUILayout.Width(50));
                    LevelObjectSet objSet = levelSets.levelSets[editingLevelSetIndex].startSets[i];

                    Debug.Log($"Obj Type Change:{objSet.objType}");
                    int startTypeIndex = EditorGUILayout.Popup(GetTypeIndex(startPath, objSet.objType), startPath);
                    int objPathIndex;
                    if (objSet.objType != startPath[startTypeIndex])
                    {
                        objSet.objType = startPath[startTypeIndex];
                        objPathIndex = 0;
                    }

                    if(objSet.objType != "none")
                    {
                        string[] objPath = GetObjectPathOfType("Start\\", objSet.objType);
                        objPathIndex = GetPathIndex(objPath, objSet.objType, objSet.objPath);
                        int newObjPathIndex = EditorGUILayout.Popup(objPathIndex, objPath);
                        objSet.objPath = GetPathOfIndex(newObjPathIndex, objPath, "Prefabs/Level Elements/Start/" + objSet.objType + "/");
                    }
                    else
                    {
                        EditorGUILayout.Popup(0, new[] { "none" });
                    }

                    int basePathIndex = EditorGUILayout.Popup(GetPathIndex(startBasePath, "Start", objSet.basePath), startBasePath);
                    objSet.basePath = GetPathOfIndex(basePathIndex, startBasePath, "Prefabs/Level Elements/Plane/Start/");

                    levelSets.levelSets[editingLevelSetIndex].startSets[i] = objSet;

                    if (GUILayout.Button("X"))
                    {
                        levelSets.levelSets[editingLevelSetIndex].startSets.RemoveAt(i);
                    }

                    GUILayout.EndHorizontal();
                }


                //end
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUILayout.Label("End Sets", GUILayout.MaxWidth(145));

                if (GUILayout.Button("Add End Set"))
                {
                    LevelObjectSet objSet = new LevelObjectSet();
                    levelSets.levelSets[editingLevelSetIndex].endSets.Add(objSet);
                }
                GUILayout.EndHorizontal();

                GUILayout.BeginHorizontal();
                GUILayout.Label("Index", GUILayout.Width(50));
                GUILayout.Label("End Type");
                GUILayout.Label("Object Path");
                GUILayout.Label("Base Path");
                GUILayout.Label(" ", GUILayout.Width(30));
                GUILayout.EndHorizontal();

                for (int i = 0; i < levelSets.levelSets[editingLevelSetIndex].endSets.Count; i++)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label($"Set {i}", GUILayout.Width(50));
                    LevelObjectSet objSet = levelSets.levelSets[editingLevelSetIndex].endSets[i];

                    Debug.Log($"Obj Type Change:{objSet.objType}");
                    int startTypeIndex = EditorGUILayout.Popup(GetTypeIndex(endPath, objSet.objType), endPath);
                    int objPathIndex;
                    if (objSet.objType != endPath[startTypeIndex])
                    {
                        objSet.objType = endPath[startTypeIndex];
                        objPathIndex = 0;
                    }

                    if (objSet.objType != "none")
                    {
                        string[] objPath = GetObjectPathOfType("End\\", objSet.objType);
                        objPathIndex = GetPathIndex(objPath, objSet.objType, objSet.objPath);
                        int newObjPathIndex = EditorGUILayout.Popup(objPathIndex, objPath);
                        objSet.objPath = GetPathOfIndex(newObjPathIndex, objPath, "Prefabs/Level Elements/End/" + objSet.objType + "/");
                    }
                    else
                    {
                        EditorGUILayout.Popup(0, new[] { "none" });
                    }

                    int basePathIndex = EditorGUILayout.Popup(GetPathIndex(endBasePath, "End", objSet.basePath), endBasePath);
                    objSet.basePath = GetPathOfIndex(basePathIndex, endBasePath, "Prefabs/Level Elements/Plane/End/");

                    levelSets.levelSets[editingLevelSetIndex].endSets[i] = objSet;

                    if (GUILayout.Button("X"))
                    {
                        levelSets.levelSets[editingLevelSetIndex].endSets.RemoveAt(i);
                    }

                    GUILayout.EndHorizontal();
                }

                //add
                GUILayout.Space(30);
                if (GUILayout.Button("Create A Level Set"))
                {
                    CreateALevelSet();
                }

                //save
                if (GUILayout.Button("Save Level Sets"))
                {
                    SaveRegisteredLevelSetInfo();
                }
            }
            
        }
        #endregion
    }

    void OnEnable()
    {
        //Register call back
        SceneView.duringSceneGui += OnSceneGUI;

        //Load Prefab
        blockPrefab = Resources.Load<GameObject>("Prefabs/Level Elements/PrefPuzzleBlock");
        levelPrefab = Resources.Load<GameObject>("Prefabs/PrefLevel");

        //Initialize Level Set Info
        levelSetNames = new List<string>();
        levelSetIndices = new List<int>();
        LoadRegisteredLevelSetInfo();

        //Initialize Level Element
        GetLevelElements();

        //Initialize Selecting Levels
        InitializeLevels();

        //Initialize Camera
        playCamera = GameObject.Find("Camera").GetComponent<Camera>();

    }

    void OnDisable()
    {
        SaveRegisteredLevelSetInfo();
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    #region Level Management

    private void InitializeLevels()
    {
        //Initialize Level Info
        levelParents = new List<Transform>();
        levelNames = new List<string>();
        levelIndices = new List<int>();
        FindLevelsInScene();

        //Initialize selecting level info
        selectingParentIndex = 0;
        if (levelParents.Count > 0)
        {
            UpdateUsingLevelSet(levelParents[0].GetComponent<Level>().levelSetID);
        }

        if (levelParents.Count > 0)
        {
            selectingParentIndex = 0;
            GetBlockCount(levelParents[0]);
        }
    }

    private void FindLevelsInScene()    //Initialiaze Level
    {
        GameObject[] objects = GameObject.FindGameObjectsWithTag("Level");
        levelParents.Clear();
        levelNames.Clear();
        levelIndices.Clear();
        Regex regex = new Regex(@"(?<=Level - )[\w\W]+");
        for (int i = 0; i < objects.Length; i++)
        {
            levelParents.Add(objects[i].transform);
            levelNames.Add(regex.Match(objects[i].name).Value);
            levelIndices.Add(i);
        }
    }

    private void ChangeLevelName(int index, string newName) //Change Level Name
    {
        if (newName == levelNames[index])
        {
            return;
        }

        Regex r1 = new Regex(@"^" + newName + @" \(\d+\)$");
        Regex r2 = new Regex(@"(?<=\()\d+(?=\))");
        int maxCount = 0;
        bool needChangeName = false;
        for (int i = 0; i < levelNames.Count; i++)
        {
            if (i == index)
            {
                continue;
            }

            if (levelNames[i] == newName)
            {
                needChangeName = true;
            }

            Match m1 = r1.Match(levelNames[i]);
            if (m1.Success)
            {
                int num = Convert.ToInt32(r2.Match(m1.Value).Value);
                if (maxCount < num)
                {
                    maxCount = num;
                }
            }
        }

        maxCount += 1;
        if (needChangeName)
        {
            levelNames[index] = $"{newName} ({maxCount})";
            levelParents[index].name = "Level - " + levelNames[index];
        }
        else
        {
            levelNames[index] = newName;
            levelParents[index].name = "Level - " + newName;
        }
    }

    private void AddNewLevel()
    {
        GameObject levelObject = Instantiate(levelPrefab);
        Level level = levelObject.GetComponent<Level>();
        level.levelSetID = levelSets.levelSets[selectingLevelSetIndex].ID;

        Regex r1 = new Regex(@"^" + defaultLevelName + @"\d+$");
        Regex r2 = new Regex(@"(?<=" + defaultLevelName + @")\d+");
        int num = 0;
        foreach (string name in levelNames)
        {
            Match m = r1.Match(name);
            if (m.Success)
            {
                int n = Convert.ToInt32(r2.Match(m.Value).Value);
                if (n > num)
                {
                    num = n;
                }
            }
        }
        num += 1;
        levelObject.name = $"Level - {defaultLevelName}{num}";
        levelParents.Add(levelObject.transform);
        levelNames.Add($"{defaultLevelName}{num}");
        levelIndices.Add(levelIndices.Count);
    }

    private void UpdateSelectingLevelInfo(int index)
    {
        if(selectingParentIndex == index)
        {
            return;
        }
        selectingParentIndex = index;
        UpdateUsingLevelSet(levelParents[index].GetComponent<Level>().levelSetID);
    }

    private void UpdateLevels()
    {
        if(levelParents.Count == 0)
        {
            return;
        }

        Transform choosingTransform = levelParents[selectingParentIndex];
        int index = 0;
        int count = levelParents.Count;
        for(int i = 0; i < count; i++)
        {
            if(levelParents[index] == null)
            {
                levelParents.RemoveAt(index);
                levelNames.RemoveAt(index);
            }
            else
            {
                index++;
            }
        }

        if(levelParents.Count == 0)
        {
            selectingParentIndex = 0;
            return;
        }

        if(choosingTransform == null)
        {
            selectingParentIndex = 0;
            UpdateUsingLevelSet(levelParents[0].GetComponent<Level>().levelSetID);
        }
        else
        {
            selectingParentIndex = levelParents.IndexOf(choosingTransform);
        }

        if(levelIndices.Count != levelParents.Count)
        {
            levelIndices.Clear();
            for(int i = 0; i < levelParents.Count; i++)
            {
                levelIndices.Add(i);
            }
        }
    }

    #endregion

    #region Level Edit

    private void GetBlockCount(Transform parent)
    {
        blockCount = 0;
        Regex regex = new Regex(@"(?<=" + blockName + @")\d+");
        for (int i = 0; i < parent.childCount; i++)
        {
            string childName = parent.GetChild(i).name;
            Match m = regex.Match(childName);
            if (m.Success)
            {
                int num = Convert.ToInt32(m.Value);
                if (blockCount < num)
                {
                    blockCount = num;
                }
            }
        }
        blockCount += 1;
    }

    private void RaycastInDirectionAndAddBlock(Vector3 n, Vector3 p, PuzzleBlock newBlock)
    {
        RaycastHit hit;
        Quaternion rotation = levelParents[selectingParentIndex].rotation;
        if (Physics.Raycast(p, rotation * n, out hit, 0.6f, 1 << 8))
        {
            //if hit, get the hit block
            PuzzleBlock block = hit.collider.GetComponentInParent<PuzzleBlock>();
            //if not in this level group, ignore
            if (block.transform.parent != levelParents[selectingParentIndex].Find("Blocks")) return;
            //get direction
            Direction dir = block.GetDirectionOfHitPoint(hit.point);
            //Add block
            block.AddBlockTo(dir, newBlock);
        }
    }

    private void UpdateSelectingBlock()
    {
        // Shoot a ray from the mouse position into the world
        Ray worldRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        RaycastHit hit;
        // Shoot this ray. check in a distance of 10000.
        if (Physics.Raycast(worldRay, out hit, 10000))
        {
            if (hit.collider.CompareTag("Puzzle"))
            {
                selectingBlock = hit.collider.GetComponent<PuzzleBlock>();
                if (selectingBlock.transform.parent != levelParents[selectingParentIndex].Find("Blocks"))
                {
                    selectingBlock = null;
                }
                else
                {
                    selectingDirection = selectingBlock.GetDirectionOfHitPoint(hit.point);
                    highLightPoints = selectingBlock.GetPointsForView(selectingDirection);
                }
            }
        }
        else
        {
            selectingBlock = null;
        }
    }

    private void UpdateLevelSetObjectNames()
    {
        LevelSet set = levelSets.levelSets[selectingLevelSetIndex];
        startTypeNames = new string[set.startSets.Count];
        for(int i = 0; i < startTypeNames.Length; i++)
        {
            startTypeNames[i] = set.startSets[i].objType;
        }
        endTypeNames = new string[set.endSets.Count];
        for(int i = 0;i < endTypeNames.Length; i++)
        {
            endTypeNames[i] = set.endSets[i].objType;
        }
    }

    private void ChangeViewToTargetRotation(ObjectRotate obj, int index)
    {
        //Set The View To The Player's Perspective 
        SceneView.lastActiveSceneView.pivot = playCamera.transform.position;
        SceneView.lastActiveSceneView.rotation = playCamera.transform.rotation;
        SceneView.lastActiveSceneView.orthographic = true;
        SceneView.lastActiveSceneView.size = playCamera.orthographicSize;

        //Set the level rotation
        obj.RotateTo(obj.eventAngles[index]);
        eventAngle = obj.eventAngles[index];

        //Repaint The Scene
        SceneView.lastActiveSceneView.Repaint();
    }
    #endregion

    #region LevelSet Management

    private void LoadRegisteredLevelSetInfo()
    {
        string wholeDataPath = Directory.GetCurrentDirectory() + dataPath;
        if (!File.Exists(wholeDataPath))
        {
            levelSets = new LevelSets();
        }
        else
        {
            string levelSetsInfo = File.ReadAllText(wholeDataPath);
            levelSets = JsonUtility.FromJson<LevelSets>(levelSetsInfo);
        }

        List<LevelSet> levelSetList = levelSets.levelSets;
        for (int i = 0; i < levelSetList.Count; i++) 
        {
            levelSetNames.Add(levelSetList[i].setName);
            levelSetIndices.Add(i);
        }
    }

    private void SaveRegisteredLevelSetInfo()
    {
        string wholeDataPath = Directory.GetCurrentDirectory() + dataPath;
        string json = JsonUtility.ToJson(levelSets);
        File.WriteAllText(wholeDataPath, json);
    }

    private void UpdateUsingLevelSet(int levelSetID)
    {
        if(levelSets.levelSets.Count == 0)
        {
            Debug.Log($"Level Set is Empty!!!!");
            return;
        }
        for (int i = 0; i < levelSets.levelSets.Count; i++)
        {
            if (levelSets.levelSets[i].ID == levelSetID)
            {
                selectingLevelSetIndex = i;
                UpdateLevelSetObjectNames();
                return;
            }
        }
        Debug.LogError($"There is no Level Set ID = {levelSetID}");
    }

    private void ChangeLevelSet(int index)
    {
        if(selectingLevelSetIndex == index)
        {
            return;
        }

        if (!CheckSetCompatible(index))
        {
            Debug.LogError($"{levelSets.levelSets[index].setName} is not compatible with current level!");
        }

        LevelGrid[] grids = levelParents[selectingParentIndex].Find("Blocks").GetComponentsInChildren<LevelGrid>();
        LevelSet set = levelSets.levelSets[index];
        foreach(LevelGrid grid in grids)
        {
            grid.defaultModelPath = set.notSprayablePath;
            grid.frameFilePath = set.framePath;
            grid.planeFilePath = set.planePath;
            if(grid.type == GridType.START)
            {
                string name = grid.objectName;
                for(int i = 0;i < set.startSets.Count; i++)
                {
                    if(set.startSets[i].objType == name)
                    {
                        grid.objectFilePath = set.startSets[i].objPath;
                        grid.objectBaseFilePath = set.startSets[i].basePath;
                        break;
                    }
                }
            }
            else if(grid.type == GridType.END)
            {
                string name = grid.objectName;
                for (int i = 0; i < set.endSets.Count; i++)
                {
                    if (set.endSets[i].objType == name)
                    {
                        grid.objectFilePath = set.endSets[i].objPath;
                        grid.objectBaseFilePath = set.endSets[i].basePath;
                        break;
                    }
                }
            }
            grid.UpdateVisual();
        }

        levelParents[selectingParentIndex].GetComponent<Level>().levelSetID = set.ID;
        UpdateLevelSetObjectNames();
    }

    private bool CheckSetCompatible(int index)
    {
        LevelSet set = levelSets.levelSets[index];
        List<LevelObjectSet> startSets = set.startSets;
        List<LevelObjectSet> endSets = set.endSets;
        LevelGrid[] grids = levelParents[selectingParentIndex].Find("Blocks").GetComponentsInChildren<LevelGrid>();
        foreach(LevelGrid grid in grids)
        {
            if(grid.type == GridType.START)
            {
                bool found = false;
                foreach(LevelObjectSet objectSet in startSets)
                {
                    if(objectSet.objType == grid.objectName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }else if(grid.type == GridType.END)
            {
                bool found = false;
                foreach(LevelObjectSet objectSet in endSets)
                {
                    if(objectSet.objType == grid.objectName)
                    {
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    return false;
                }
            }
        }
        return true;
    }

    private void CreateALevelSet()
    {
        LevelSet levelSet = new LevelSet();
        Regex r1 = new Regex(@"^" + defaltLevelSetName + @"\d+$");
        Regex r2 = new Regex(@"(?<=" + defaltLevelSetName + @")\d+");
        int num = 0;
        foreach(LevelSet set in levelSets.levelSets)
        {
            Match m = r1.Match(set.setName);
            if (m.Success)
            {
                int n = Convert.ToInt32(r2.Match(m.Value).Value);
                if(n > num)
                {
                    num = n;
                }
            }
        }
        num += 1;
        levelSet.ID = levelSets.nextLevelSetID++;
        levelSet.setName = $"{defaltLevelSetName}{num}";
        levelSets.levelSets.Add(levelSet);
        levelSetNames.Add(levelSet.setName);
        levelSetIndices.Add(levelSetIndices.Count);
    }

    private void ChangeSetName(int index, string newName)
    {
        if (newName == levelSetNames[index])
        {
            return;
        }

        Regex r1 = new Regex(@"^" + newName + @" \(\d+\)$");
        Regex r2 = new Regex(@"(?<=\()\d+(?=\))");
        int maxCount = 0;
        bool needChangeName = false;
        for (int i = 0; i < levelSetNames.Count; i++)
        {
            if (i == index)
            {
                continue;
            }

            if (levelSetNames[i] == newName)
            {
                needChangeName = true;
            }

            Match m1 = r1.Match(levelSetNames[i]);
            if (m1.Success)
            {
                int num = Convert.ToInt32(r2.Match(m1.Value).Value);
                if (maxCount < num)
                {
                    maxCount = num;
                }
            }
        }

        maxCount += 1;
        if (needChangeName)
        {
            levelSetNames[index] = $"{newName} ({maxCount})";
            levelSets.levelSets[index].setName = levelNames[index];
        }
        else
        {
            levelSetNames[index] = newName;
            levelSets.levelSets[index].setName = newName;
        }
    }

    private void GetLevelElements()
    {
        notSprayablePath = GetDirectories("NotSprayable");
        framePath = GetDirectories("Frame");
        planePath = GetDirectories("Plane\\Normal", "Normal");
        startBasePath = GetDirectories("Plane\\Start", "Start");
        endBasePath = GetDirectories("Plane\\End", "End");
        startPath = GetDirectories("Start");
        endPath = GetDirectories("End");
    }

    private string[] GetDirectories(string path,string lastfile = "")//默认到
    {
        if(lastfile == "")
        {
            lastfile = path;
        }

        //Debug.Log(path);
        //Debug.Log(lastfile);

        string wholePrefabPath = Directory.GetCurrentDirectory() + prefabPath + path;
        string[] p = Directory.GetDirectories(wholePrefabPath);
        string[] ret = new string[p.Length + 1];
        Regex regex = new Regex(@"(?<=" + lastfile + @"\\)[\w\W]+$");
        ret[0] = "none";
        for (int i = 0; i < p.Length; i++) 
        {
            ret[i + 1] = regex.Match(p[i]).Value;
            //Debug.Log(ret[i + 1]);
        }
        return ret;
    }

    private int GetTypeIndex(string[] types,string name)
    {
        for(int i = 0; i < types.Length; i++)
        {
            if(types[i] == name)
            {
                return i;
            }
        }
        return 0;
    }

    private int GetPathIndex(string[] path, string pre, string name)
    {
        //Debug.Log($"[GetPathIndex]{name}");
        for(int i = 0; i< path.Length; i++)
        {
            Regex regex = new Regex(@"(?<=" + pre + @"/)[\w\W]+(?=/)");
            string result = regex.Match(name).Value;
            Debug.Log($"[GetPathIndex]{result}");
            if (result == path[i])
            {
                Debug.Log($"Match Index of {name} is {i}");
                return i;
            }
        }
        return 0;
    }

    private string GetPathOfIndex(int index, string[] path, string wholePath)
    {
        if(index == 0)
        {
            return path[0];
        }
        else
        {
            string p = Directory.GetCurrentDirectory() + "\\Assets\\Resources\\" + wholePath + path[index];
            string[] filename = Directory.GetFiles(p);
            string prefabName;
            if(filename.Length > 0)
            {
                //Debug.Log($"[FileName]{filename[0]}");
                Regex r = new Regex(@"(?<=" + path[index] + @"\\)[\W\w]+_");
                prefabName = r.Match(filename[0]).Value;
                //Debug.Log(wholePath + path[index] + "/" + prefabName);
                return wholePath + path[index] + "/" + prefabName;
            }
            else
            {
                Debug.LogError("This Directory Contains Nothing!");
                return path[0];
            }

        }
    }

    private string[] GetObjectPathOfType(string path, string type)
    {
        //Debug.Log($"[GetObjectPathOfType]Path:{path}");
        //Debug.Log($"[GetObjectPathOfType]Type:{type}");
        return GetDirectories(path + type, type);
    }

    #endregion

    #region Visualization
    private void UpdateDebugInfo()
    {
        if (selectingBlock == null)
        {
            debugInfo = "Selecting Grid : none";
            return;
        }
        LevelGrid grid = selectingBlock.GetGrid(selectingDirection);
        debugInfo = $"Selecting Block : {selectingBlock.name}\n" +
            $"Grid Direction : {selectingDirection}\n" +
            $"Grid Type : {grid.type}\n" +
            $"Grid Status : {Convert.ToString(grid.gridStatus, 2).PadLeft(14, '0')}\n" +
            $"Object Status : {Convert.ToString(grid.objectStatus, 2).PadLeft(5, '0')}\n" +
            $"Near Grids\n";

        for (int i = 1; i <= 4; i++)
        {
            LevelGrid tmp = grid.GetNearGrid((Direction)i);
            if (tmp == null)
            {
                debugInfo += $"{(Direction)i}:none\n";
            }
            else
            {
                debugInfo += $"{(Direction)i}:{tmp.block.name} - {tmp.direction}\n";
            }

        }

        debugInfo += "Covering Grid :";

        if (grid.coveringGrid == null)
        {
            debugInfo += " none";
        }
        else
        {
            debugInfo += $" {grid.coveringGrid.block.name} - {grid.coveringGrid.direction}";
        }

        if (grid.isSprayable())
        {
            debugInfo += "\nIsSprayable:true";
        }
        else
        {
            debugInfo += "\nIsSprayable:false";
        }

        debugInfo += $"\nDefaultPath = {grid.defaultModelPath}";
        debugInfo += $"\nPlanePath = {grid.planeFilePath}";
        debugInfo += $"\nFramePath = {grid.frameFilePath}";
        debugInfo += $"\nObjectPath = {grid.objectFilePath}";
        debugInfo += $"\nObjectBasePath = {grid.objectBaseFilePath}";

        debugInfo += $"\nUpdate Status : {Convert.ToString(grid.updateStatus, 2).PadLeft(12, '0')}";
        debugInfo += $"\nObject Update Status : {Convert.ToString(grid.updateObjectStatus, 2).PadLeft(12, '0')}";
    }

    private void PaintSquare(Vector3[] points, Color color)
    {
        Handles.color = color;
        for (int i = 0; i < 3; i++)
        {
            Handles.DrawLine(points[i], points[i + 1]);
        }
        Handles.DrawLine(points[3], points[0]);
        HandleUtility.Repaint();
    }

    #endregion
}