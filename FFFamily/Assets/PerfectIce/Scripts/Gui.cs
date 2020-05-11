using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Gui : MonoBehaviour
{
  public Material iceCube;
  public Transform FreezeObject;
  public Transform Target;
  public float FreezeTime = 2;
  public Texture[] Textures;
  public Texture[] NRMTextures;
  private float cutoff = -1.0f;
  private float Cutoff = 1.0f;
  private float RefStrength = 3.5f;
  private float LightStrength = 1.0f;
  private float FrenelPower = 5.0f;
  private float TexAlphaAdd = 0.1f;
  private float RefractionStrength = 1.0f;
  private Rect windowRect = new Rect(16, 16, 280, 315);
  public HoriPivot HCamera;
  public VertPivot VCamera;
  private Vector2 dragStart;
  private Vector3 dragStartAngle;
  private Vector3 dragLastAngle;
  private int frozen, i = 1, j = 1;

	void Update () 
	{
		bool ingui = MouseIsInRect(Input.mousePosition, windowRect);
		if (ingui) {HCamera.TrueOrbit = false; VCamera.TrueOrbit = false;}
		if(!ingui) {HCamera.TrueOrbit = true; VCamera.TrueOrbit = true;}
		if(frozen==1)MaterialFreeze();
		if(frozen==0)MaterialUnFreeze();
	}

	private bool MouseIsInRect(Vector3 pt, Rect rect)
	{
		var x = pt.x;
		var y = Screen.height - pt.y;
		return (x >= rect.x && x <= rect.x + rect.width && y >= rect.y && y <= rect.y + rect.height);
	}

	private bool PointInRect(Vector2 p, Rect r)
	{
		return  (p.x>=r.xMin && p.x<=r.xMax && p.y>=r.yMin && p.y<=r.yMax);
	}

  private void OnGUI()
	{
		windowRect = GUI.Window (0, windowRect, WindowFunction, "Demo");

		RefStrength = GUI.HorizontalSlider (new Rect (175, 85, 100, 30), RefStrength, 0.0f, 20.0f);
		iceCube.SetFloat("_RefStrength", RefStrength);

		LightStrength = GUI.HorizontalSlider (new Rect (175, 115, 100, 30), LightStrength, 0.0f, 20.0f);
		iceCube.SetFloat("_LightStrength", LightStrength);

		FrenelPower = GUI.HorizontalSlider (new Rect (175, 145, 100, 30), FrenelPower, 0.1f, 5.0f);
		iceCube.SetFloat("_FrenelPower", FrenelPower);

		TexAlphaAdd = GUI.HorizontalSlider (new Rect (175, 175, 100, 30), TexAlphaAdd, -1.0f, 1.0f);
		iceCube.SetFloat("_TexAlphaAdd", TexAlphaAdd);

		RefractionStrength = GUI.HorizontalSlider (new Rect (175, 205, 100, 30), RefractionStrength, -10.0f, 10.0f);
		iceCube.SetFloat("_RefractionStrength", RefractionStrength);

		Cutoff = GUI.HorizontalSlider (new Rect (175, 235, 100, 30), Cutoff, -1.0f, 1.0f);
		iceCube.SetFloat("_Cutoff", Cutoff);
    }

	void WindowFunction (int windowID) 
	{
		GuiNotes ();
	}

	private void GuiNotes()
	{
		GUILayout.BeginVertical("box");
		
		var centeredStyle = GUI.skin.GetStyle("Label");
		centeredStyle.alignment = TextAnchor.UpperLeft;
		GUILayout.Label("Rotate: Left mouse | Zoom: Scroll wheel", centeredStyle);
		GUILayout.EndVertical();
		GUI.Label (new Rect (20, 65, 120, 30), "Reflection Strength");
		GUI.Label (new Rect (20, 95, 100, 30), "Light Strength");
		GUI.Label (new Rect (20, 125, 100, 30), "Frenel Power");
		GUI.Label (new Rect (20, 153, 100, 30), "TexAlphaAdd");
		GUI.Label (new Rect (20, 182, 120, 30), "Refraction Strength");
		GUI.Label (new Rect (20, 212, 100, 30), "Cutoff");

		if (GUI.Button (new Rect (153, 280, 110, 25), "Reset scene")) 
		{
			/*RefStrength = 3.5f;
			LightStrength = 1.0f;
			FrenelPower = 5.0f;
			TexAlphaAdd = 0.1f;
			RefractionStrength = 1.0f;
			Cutoff = 1.0f; 
			Target.transform.rotation = Quaternion.Euler(3.732184f, 175.4815f, 28.30005f);*/
			iceCube.mainTexture = Textures[0];
			iceCube.SetTexture("_Refraction", NRMTextures[0]);
			SceneManager.LoadScene (Application.loadedLevelName);
		}

		if (GUI.Button (new Rect (18, 240, 110, 25), "Change Texture")) 
		{
			if(i==Textures.Length) i=0;
			iceCube.mainTexture = Textures[i++];
		}

		if (GUI.Button (new Rect (153, 240, 110, 25), "Change Normal"))
		{
		   if(j==NRMTextures.Length) j=0;
		   iceCube.SetTexture("_Refraction", NRMTextures[j++]);
		}

		if (GUI.Button (new Rect (18, 280, 110, 25), "Freeze/Defrost")) 
		{
			frozen++;
			frozen = frozen%2;
		}
	}

	void MaterialUnFreeze()
	{
		var time = Time.deltaTime / FreezeTime;
		if (cutoff-time >= -1) 
		{
			cutoff -= time;
			FreezeObject.GetComponent<Renderer>().material.SetFloat ("_Cutoff", cutoff);
		}
		frozen = 0;
	}

	void MaterialFreeze()
	{
		var time = Time.deltaTime / FreezeTime;
		if (cutoff + time <= 1) 
		{
			cutoff += time;
			FreezeObject.GetComponent<Renderer>().material.SetFloat ("_Cutoff", cutoff);
		}
	}
}
