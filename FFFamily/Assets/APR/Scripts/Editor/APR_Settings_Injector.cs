using UnityEngine;
using UnityEditor;

public class APR_Settings_Injector : EditorWindow
{
	//Editor
    public Texture tex;
    private static APR_Settings_Injector _instance;
    
    [MenuItem("APR/APR Settings Injector")]
    static void APRInjectorWindow()
    {
        if(_instance == null)
        {
            APR_Settings_Injector window = ScriptableObject.CreateInstance(typeof(APR_Settings_Injector)) as APR_Settings_Injector;
            window.maxSize = new Vector2(350f, 180f);
            window.minSize = window.maxSize;
            window.ShowUtility();
        }
    }
    
    void OnEnable()
    {
        _instance = this;
    }
    
	
	void OnGUI()
	{ 
		GUI.skin.label.wordWrap = true;
		
        EditorGUILayout.Space();
        GUILayout.Label(tex);
        
		EditorGUILayout.Space();
		EditorGUILayout.Space();
        
		GUILayout.Label("Inject Preferred Active Physics Ragdoll Project Settings");
		
		EditorGUILayout.Space();
		if(GUILayout.Button("Inject APR Settings"))
		{
			Physics.gravity = new Vector3(0, -25, 0);
            Physics.defaultSolverIterations = 12;
            Physics.defaultSolverVelocityIterations = 6;
            
			Debug.Log("APR settings has been successfully injected");
            
            this.Close();
		}
	}
    
    void OnDisable()
    {
        _instance = null;
    }
}
