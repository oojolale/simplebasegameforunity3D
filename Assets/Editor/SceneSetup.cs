using UnityEngine;
using UnityEditor;

/// <summary>
/// 编辑器工具：一键生成草坪 + 几何体人物 + 石头城墙 + 梯田 + 树木
/// 菜单：Tools → 创建草坪和人物场景
///       Tools → 升级场景（石墙/梯田/树木）
/// </summary>
public class SceneSetup : EditorWindow
{
    // ═══════════════════════════════════════════════════
    //  公共材质缓存（整个 CreateScene 生命周期内共享）
    // ═══════════════════════════════════════════════════
    static Material s_grassMat;
    static Material s_skinMat;
    static Material s_clothMat;
    static Material s_pantsMat;
    static Material s_shoesMat;
    static Material s_stoneMat;
    static Material s_soilMat;
    static Material s_trunkMat;
    static Material s_leafMat;

    // ═══════════════════════════════════════════════════
    //  菜单 1：完整重建（草坪 + 人物 + 景观）
    // ═══════════════════════════════════════════════════
    [MenuItem("Tools/创建草坪和人物场景")]
    public static void CreateScene()
    {
        // ── 清理旧物体 ─────────────────────────────────
        foreach (string name in new[]{ "Lawn","Player","StoneWalls","Landscape" })
        {
            GameObject old = GameObject.Find(name);
            if (old != null) DestroyImmediate(old);
        }

        // ── 确保目录 ───────────────────────────────────
        EnsureMaterialsFolder();

        // ── 材质 ───────────────────────────────────────
        BuildMaterials();

        // ── 草坪（扩大两倍：scale 6→60×60 单位）────────
        GameObject lawn = GameObject.CreatePrimitive(PrimitiveType.Plane);
        lawn.name = "Lawn";
        lawn.transform.position   = Vector3.zero;
        lawn.transform.localScale = new Vector3(6f, 1f, 6f);
        lawn.GetComponent<Renderer>().sharedMaterial = s_grassMat;

        // ── 人物 ───────────────────────────────────────
        BuildPlayer();

        // ── 景观（城墙 + 梯田 + 树木）────────────────
        BuildLandscape();

        // ── 摄像机 ─────────────────────────────────────
        SetupCamera(GameObject.Find("Player"));

        // ── 刷新 ───────────────────────────────────────
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("✅ 场景创建完成！按 Play 后使用方向键/WASD 控制人物移动。");
        EditorUtility.DisplayDialog("创建成功",
            "场景已创建完成！\n\n按 Play 后使用【方向键 或 WASD】控制人物移动。", "OK");
    }

    // ═══════════════════════════════════════════════════
    //  菜单 2：仅升级景观（保留现有人物）
    // ═══════════════════════════════════════════════════
    [MenuItem("Tools/升级场景（石墙_梯田_树木）")]
    public static void UpgradeScene()
    {
        // 扩大草坪
        GameObject lawn = GameObject.Find("Lawn");
        if (lawn != null) lawn.transform.localScale = new Vector3(6f, 1f, 6f);

        // 清理旧景观
        GameObject old = GameObject.Find("StoneWalls");
        if (old != null) DestroyImmediate(old);
        old = GameObject.Find("Landscape");
        if (old != null) DestroyImmediate(old);

        EnsureMaterialsFolder();
        BuildMaterials();
        BuildLandscape();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("✅ 景观升级完成！");
        EditorUtility.DisplayDialog("升级成功", "石墙、梯田、树木已添加完成！", "OK");
    }

    // ═══════════════════════════════════════════════════
    //  草坪扩大、材质、人物、景观 各模块
    // ═══════════════════════════════════════════════════

    static void EnsureMaterialsFolder()
    {
        if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            AssetDatabase.CreateFolder("Assets", "Materials");
    }

    // 创建或加载所有材质
    static void BuildMaterials()
    {
        s_grassMat = MakeMat("GrassMaterial",  new Color(0.18f, 0.65f, 0.18f));
        s_skinMat  = MakeMat("SkinMaterial",   new Color(0.95f, 0.75f, 0.60f));
        s_clothMat = MakeMat("ClothMaterial",  new Color(0.20f, 0.40f, 0.80f));
        s_pantsMat = MakeMat("PantsMaterial",  new Color(0.25f, 0.20f, 0.55f));
        s_shoesMat = MakeMat("ShoesMaterial",  new Color(0.15f, 0.10f, 0.10f));
        s_stoneMat = MakeMat("StoneMaterial",  new Color(0.55f, 0.52f, 0.48f));
        s_soilMat  = MakeMat("SoilMaterial",   new Color(0.55f, 0.38f, 0.18f));
        s_trunkMat = MakeMat("TrunkMaterial",  new Color(0.40f, 0.25f, 0.10f));
        s_leafMat  = MakeMat("LeafMaterial",   new Color(0.15f, 0.55f, 0.15f));
    }

    // 创建或覆盖一个材质资产
    static Material MakeMat(string assetName, Color color)
    {
        string path = $"Assets/Materials/{assetName}.mat";
        Material existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null)
        {
            existing.color = color;
            return existing;
        }
        Material mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }

    // ── 人物 ──────────────────────────────────────────
    static void BuildPlayer()
    {
        GameObject player = new GameObject("Player");
        player.transform.position = new Vector3(0f, 0f, 0f);

        float footH = 0.25f, legH = 0.5f, bodyH = 0.8f, headH = 0.4f;
        float legBottomY  = footH;
        float bodyBottomY = legBottomY  + legH;
        float headBottomY = bodyBottomY + bodyH;
        float bodyWidth   = 0.5f, bodyDepth = 0.3f;

        // 头
        CreatePart("Head",     player.transform,
            new Vector3(0f, headBottomY + headH * 0.5f, 0f),
            new Vector3(headH, headH, headH), s_skinMat);
        // 身体
        CreatePart("Body",     player.transform,
            new Vector3(0f, bodyBottomY + bodyH * 0.5f, 0f),
            new Vector3(bodyWidth, bodyH, bodyDepth), s_clothMat);
        // 双臂
        float armW = 0.18f, armH = 0.55f, armD = 0.18f;
        float armY = bodyBottomY + bodyH * 0.72f - armH * 0.5f;
        CreatePart("ArmLeft",  player.transform,
            new Vector3(-(bodyWidth*0.5f+armW*0.5f), armY, 0f),
            new Vector3(armW, armH, armD), s_skinMat);
        CreatePart("ArmRight", player.transform,
            new Vector3( (bodyWidth*0.5f+armW*0.5f), armY, 0f),
            new Vector3(armW, armH, armD), s_skinMat);
        // 双腿
        float legW = 0.20f, legD = 0.22f;
        CreatePart("LegLeft",  player.transform,
            new Vector3(-legW*0.5f-0.02f, legBottomY+legH*0.5f, 0f),
            new Vector3(legW, legH, legD), s_pantsMat);
        CreatePart("LegRight", player.transform,
            new Vector3( legW*0.5f+0.02f, legBottomY+legH*0.5f, 0f),
            new Vector3(legW, legH, legD), s_pantsMat);
        // 双脚
        float shoeH=0.20f, shoeW=0.18f, shoeD=0.28f;
        CreatePart("FootLeft",  player.transform,
            new Vector3(-legW*0.5f-0.02f, footH*0.5f, 0.03f),
            new Vector3(shoeW, shoeH, shoeD), s_shoesMat);
        CreatePart("FootRight", player.transform,
            new Vector3( legW*0.5f+0.02f, footH*0.5f, 0.03f),
            new Vector3(shoeW, shoeH, shoeD), s_shoesMat);

        // Rigidbody
        Rigidbody rb = player.AddComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // 胶囊碰撞器
        CapsuleCollider cap = player.AddComponent<CapsuleCollider>();
        float totalH = headBottomY + headH;
        cap.center = new Vector3(0f, totalH * 0.5f, 0f);
        cap.height = totalH;
        cap.radius = 0.28f;

        player.AddComponent<PlayerController>();
    }

    // ── 整体景观根节点 ────────────────────────────────
    static void BuildLandscape()
    {
        GameObject landscapeRoot = new GameObject("Landscape");

        BuildStoneWalls(landscapeRoot.transform);
        BuildTerraces(landscapeRoot.transform);
        BuildTrees(landscapeRoot.transform);
    }

    // ═══════════════════════════════════════════════════
    //  石头城墙：沿草坪四边，高度不均，错落布置
    //  草坪 60×60，边缘在 ±30
    // ═══════════════════════════════════════════════════
    static void BuildStoneWalls(Transform parent)
    {
        GameObject wallRoot = new GameObject("StoneWalls");
        wallRoot.transform.SetParent(parent);

        float edge  = 29f;   // 石块内侧贴近草坪边缘
        float wallY = 0f;    // 石块底部在地面

        // 每面墙：沿轴方向排列多块石头，高度随机化模拟山城凹凸
        // 北墙 (Z = +edge)
        SpawnWallLine(wallRoot.transform, new Vector3(0, 0,  edge),
            true,  60, 1.8f, 2.5f, 4.5f, 0.3f);
        // 南墙 (Z = -edge)
        SpawnWallLine(wallRoot.transform, new Vector3(0, 0, -edge),
            true,  60, 1.8f, 2.5f, 4.5f, 0.3f);
        // 东墙 (X = +edge)
        SpawnWallLine(wallRoot.transform, new Vector3( edge, 0, 0),
            false, 60, 1.8f, 2.5f, 4.5f, 0.3f);
        // 西墙 (X = -edge)
        SpawnWallLine(wallRoot.transform, new Vector3(-edge, 0, 0),
            false, 60, 1.8f, 2.5f, 4.5f, 0.3f);
    }

    /// <summary>
    /// 沿一条线生成一排石块
    /// alongX=true → 沿X轴排列；false → 沿Z轴排列
    /// totalLen=总长, blockW=石块宽, minH/maxH=高度范围, depth=石块厚
    /// </summary>
    static void SpawnWallLine(Transform parent, Vector3 center,
        bool alongX, float totalLen,
        float blockW, float minH, float maxH, float depth)
    {
        int count = Mathf.RoundToInt(totalLen / blockW);
        float startOffset = -totalLen * 0.5f + blockW * 0.5f;

        // 用固定种子使每次生成结果一致
        Random.InitState(alongX ? 1234 : 5678);

        for (int i = 0; i < count; i++)
        {
            float h   = Random.Range(minH, maxH);
            float off = startOffset + i * blockW;

            // 轻微随机偏移，让石块更自然
            float jitter = Random.Range(-0.15f, 0.15f);

            Vector3 pos;
            if (alongX)
                pos = new Vector3(center.x + off + jitter,
                                  h * 0.5f,
                                  center.z + jitter * 0.5f);
            else
                pos = new Vector3(center.x + jitter * 0.5f,
                                  h * 0.5f,
                                  center.z + off + jitter);

            Vector3 scale = alongX
                ? new Vector3(blockW - 0.05f, h, depth)
                : new Vector3(depth, h, blockW - 0.05f);

            CreateScenePrimitive($"Stone_{(alongX?"X":"Z")}_{i}",
                parent, pos, scale, s_stoneMat, PrimitiveType.Cube);
        }
    }

    // ═══════════════════════════════════════════════════
    //  梯田：城中心偏角落，3~4 级阶梯土台
    // ═══════════════════════════════════════════════════
    static void BuildTerraces(Transform parent)
    {
        GameObject terraceRoot = new GameObject("Terraces");
        terraceRoot.transform.SetParent(parent);

        // 梯田组 1：右前角
        SpawnTerrace(terraceRoot.transform, new Vector3(14f, 0f,  12f), 0f);
        // 梯田组 2：左后角
        SpawnTerrace(terraceRoot.transform, new Vector3(-16f, 0f, -10f), 45f);
        // 梯田组 3：右后角（稍小）
        SpawnTerrace(terraceRoot.transform, new Vector3(18f, 0f, -15f), -20f, 0.8f);
    }

    static void SpawnTerrace(Transform parent, Vector3 basePos, float yRot,
        float sizeScale = 1f)
    {
        int levels = 4;
        for (int i = 0; i < levels; i++)
        {
            float size = (levels - i) * 3.5f * sizeScale;
            float h    = (i + 1) * 0.5f;
            float yPos = h * 0.5f;

            Vector3 offset = new Vector3(0f, 0f, -i * 1.0f * sizeScale);
            // 旋转偏移
            float rad = yRot * Mathf.Deg2Rad;
            Vector3 rotOffset = new Vector3(
                offset.x * Mathf.Cos(rad) - offset.z * Mathf.Sin(rad),
                0,
                offset.x * Mathf.Sin(rad) + offset.z * Mathf.Cos(rad));

            GameObject tier = CreateScenePrimitive($"Terrace_L{i}",
                parent,
                basePos + rotOffset + Vector3.up * yPos,
                new Vector3(size, h, size * 0.65f),
                s_soilMat, PrimitiveType.Cube);

            tier.transform.rotation = Quaternion.Euler(0, yRot, 0);

            // 梯田顶面铺一层草皮
            GameObject grassTop = CreateScenePrimitive($"TerGrass_L{i}",
                parent,
                basePos + rotOffset + Vector3.up * (h + 0.06f),
                new Vector3(size, 0.08f, size * 0.65f),
                s_grassMat, PrimitiveType.Cube);
            grassTop.transform.rotation = Quaternion.Euler(0, yRot, 0);
        }
    }

    // ═══════════════════════════════════════════════════
    //  树木：圆柱树干 + 球形树冠，错落分布
    // ═══════════════════════════════════════════════════
    static void BuildTrees(Transform parent)
    {
        GameObject treeRoot = new GameObject("Trees");
        treeRoot.transform.SetParent(parent);

        // 固定若干树木位置（错落于城内，避开人物出生点附近）
        Vector3[] treePositions = new Vector3[]
        {
            new Vector3(-8f,  0f,  10f),
            new Vector3( 5f,  0f,  18f),
            new Vector3(-20f, 0f,  5f),
            new Vector3( 22f, 0f,  8f),
            new Vector3(-12f, 0f, -18f),
            new Vector3( 10f, 0f, -20f),
            new Vector3(-25f, 0f, -12f),
            new Vector3( 20f, 0f, -5f),
            new Vector3(  2f, 0f,  25f),
            new Vector3(-5f,  0f, -25f),
            // 梯田旁的树
            new Vector3(17f,  2.0f, 10f),   // 站在梯田上
            new Vector3(-14f, 2.0f, -8f),
        };

        // 高度变化让树木有大有小
        float[] heights = { 3.5f, 4.2f, 2.8f, 5f, 3.2f, 4.5f, 3.8f, 2.5f, 4f, 3f, 2.8f, 3.5f };

        for (int i = 0; i < treePositions.Length; i++)
        {
            float th  = heights[i % heights.Length];
            float tR  = 0.18f;
            float cR  = th * 0.38f;   // 树冠半径

            Vector3 pos = treePositions[i];

            // 树干（圆柱）
            GameObject trunk = CreateScenePrimitive($"Trunk_{i}", treeRoot.transform,
                pos + Vector3.up * (th * 0.5f),
                new Vector3(tR * 2f, th, tR * 2f),
                s_trunkMat, PrimitiveType.Cylinder);

            // 树冠（球体）
            CreateScenePrimitive($"Crown_{i}", treeRoot.transform,
                pos + Vector3.up * (th + cR * 0.7f),
                new Vector3(cR * 2f, cR * 2f, cR * 2f),
                s_leafMat, PrimitiveType.Sphere);
        }
    }

    // ═══════════════════════════════════════════════════
    //  摄像机
    // ═══════════════════════════════════════════════════
    static void SetupCamera(GameObject player)
    {
        Camera mainCam = Camera.main;
        if (mainCam == null) return;

        mainCam.transform.position = new Vector3(0f, 12f, -10f);
        mainCam.transform.rotation = Quaternion.Euler(50f, 0f, 0f);

        CameraFollow cf = mainCam.gameObject.GetComponent<CameraFollow>();
        if (cf == null) cf = mainCam.gameObject.AddComponent<CameraFollow>();
        if (player != null) cf.target = player.transform;
        cf.offset = new Vector3(0f, 12f, -10f);
    }

    // ═══════════════════════════════════════════════════
    //  辅助：人物部件（Cube，去碰撞器）
    // ═══════════════════════════════════════════════════
    static GameObject CreatePart(string partName, Transform parent,
        Vector3 localPos, Vector3 localScale, Material mat)
    {
        GameObject part = GameObject.CreatePrimitive(PrimitiveType.Cube);
        part.name = partName;
        part.transform.SetParent(parent);
        part.transform.localPosition = localPos;
        part.transform.localScale    = localScale;
        part.transform.localRotation = Quaternion.identity;
        Object.DestroyImmediate(part.GetComponent<BoxCollider>());
        part.GetComponent<Renderer>().sharedMaterial = mat;
        return part;
    }

    // ═══════════════════════════════════════════════════
    //  辅助：场景摆件（保留碰撞器，用于阻挡）
    // ═══════════════════════════════════════════════════
    static GameObject CreateScenePrimitive(string objName, Transform parent,
        Vector3 worldPos, Vector3 scale, Material mat, PrimitiveType type)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = objName;
        obj.transform.SetParent(parent);
        obj.transform.position   = worldPos;
        obj.transform.localScale = scale;
        obj.GetComponent<Renderer>().sharedMaterial = mat;
        return obj;
    }
}
