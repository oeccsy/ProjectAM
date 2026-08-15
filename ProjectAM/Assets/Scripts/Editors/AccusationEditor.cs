#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

/// <summary>
/// 진행 중인 고발을 인스펙터에 보여주는 디버그용 인스펙터.
/// static 값은 직렬화되지 않으므로 여기서 직접 읽어 그린다.
/// </summary>
[CustomEditor(typeof(Accusation))]
public class AccusationEditor : Editor
{
    // 단계가 넘어가는 것을 바로 보기 위해 매 프레임 다시 그린다
    public override bool RequiresConstantRepaint() => true;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("진행 중인 고발", EditorStyles.boldLabel);

        if (Accusation.Phase == AccusationPhase.None)
        {
            EditorGUILayout.LabelField("없음");
            return;
        }

        EditorGUILayout.LabelField("단계", $"{Accusation.Phase}");
        EditorGUILayout.LabelField("고발자", $"{Accusation.Accuser.OwnColor}");
        EditorGUILayout.LabelField("대상", $"{Accusation.Accused.OwnColor}");
        EditorGUILayout.LabelField("동의", $"{Accusation.AgreedCount}명");
    }
}
#endif
