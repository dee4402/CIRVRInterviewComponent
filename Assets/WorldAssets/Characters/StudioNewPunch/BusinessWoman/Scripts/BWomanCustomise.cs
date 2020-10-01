using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BWomanCustomise : MonoBehaviour {
	private int hairCol;
	private int hairTyp;
	private int faceTyp;
	private int eyeCol;
	private int topCol;
	private int btmCol;
	private int skinTyp;
	private int topTyp;
	private int btmTyp;
	private int jacketCol;
	private int shirtCol;
	private int skirtCol;
	private int pantsCol;
	private int shoesCol;

	private BWomanMaterialsList materialsList;

	private SkinnedMeshRenderer skinnedMeshRenderer;


	public enum ShoesColor
	{
		Blue,
		Black,
		Gray,
		LightGray,
		Red,
		White

	}
	public enum JacketColor
	{
		Blue,
		Black,
		Gray,
		LightGray,
		Red,
		White

	}

	public enum SkirtColor
	{
		Blue,
		Black,
		Gray,
		LightGray,
		Red,
		White
	}

	public enum PantsColor
	{
		Blue,
		Black,
		Gray,
		LightGray,
		Red,
		White

	}

	public enum ShirtColor
	{
		Blue,
		Black,
		Gray,
		LightBlue,
		Red,
		White

	}





	public enum HairType
	{
		Medium,
		PonyTail,
		FrenchRoll,
		Short,
		Bun

	}

	public enum HairColor
	{
		Blond,
		White,
		Dark,
		Red,
		Brown

	}

	public enum EyeColor
	{
		Brown,
		Blue,
		Green,
		Black,
		DarkBlue,
		LightBrown

	}


	public enum FaceType
	{
		FaceA,
		FaceB,
		FaceC,
		FaceD,
		FaceE

	}

	public enum SkinType
	{
		Pink,
		Black,
		White,
		Tanned,
		Pale,
		Brown

	}


	public enum TopColors
	{
		WhiteBlue,
		Blue,
		Grey,
		WhitePurple

	}

	public enum TopType
	{
		Shirt,
		Jacket

	}

	public enum BottomType
	{
		Skirt,
		Pants

	}




	public FaceType faceType;
	public SkinType skinType;
	public EyeColor eyeColor;
	public TopType topType;
	public JacketColor jacketColor;
	public ShirtColor shirtColor;
	public BottomType bottomType;
	public PantsColor pantsColor;
	public SkirtColor skirtColor;

	public HairType hairType;
	public HairColor hairColor;
	public ShoesColor shoesColor;
	//public GameObject currentFace;

	public void charCustomize (int face, int eye, int topT, int bottomT, int hairT, int hairC, int skinT, int jacketC, int shirtC, int skirtC, int pantsC, int shoesC)
	{

		Material[] mat;
		materialsList = gameObject.GetComponent<BWomanMaterialsList> ();
		//Set Face
		for (int i = 0; i < materialsList.faceType.Length; i++) {
			materialsList.faceType [i].SetActive (false);
		}
		materialsList.faceType [face].SetActive (true);

		//Set Top
		for (int i = 0; i < materialsList.TopObjects.Length; i++) {
			materialsList.TopObjects [i].SetActive (false);
		}
		materialsList.TopObjects [topT].SetActive (true);

		//Set Bottom
		for (int i = 0; i < materialsList.BottomObjects.Length; i++) {
			materialsList.BottomObjects [i].SetActive (false);
			//materialsList.BottomObjects [i].GetComponent<Cloth>().enabled = true;
			materialsList.LegsObject.SetActive (false);
		}
		materialsList.BottomObjects [bottomT].SetActive (true);
		if (bottomT == 0) {
			materialsList.LegsObject.SetActive (true);
		}
		GameObject SkirtO = GameObject.Find ("Geo/Skirt/Skirt_LOD0");
		//SkirtO.GetComponent<Cloth>().enabled = false;
		//HairA==========================
		foreach (Transform child in materialsList.hairMainObjects [0].transform) {
			//print ("Foreach loop: " + child);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			//skinRend.material = materialsList.hairA_FadeMaterials [hairC];
			string oName = child.gameObject.name;
			string hName = oName.Substring (oName.Length - 1);
			//print (hName);
			mat = new Material[2];

			if (hName == "0") {
				mat [0] = materialsList.hairA_FadeMaterials [hairC];
				mat [1] = materialsList.hairA_Materials [hairC];
				skinRend.materials = mat;
			} else {
				skinRend.material = materialsList.hairA_FadeMaterials [hairC];

			}

			//mat [0] = materialsList.hairA_FadeMaterials [hairC];

		}

		//HairB==========================
		foreach (Transform child in materialsList.hairMainObjects [1].transform) {
			//print ("Foreach loop: " + child);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			//skinRend.material = materialsList.hairB_FadeMaterials [hairC];
			string oName = child.gameObject.name;
			string hName = oName.Substring (oName.Length - 1);
			//print (hName);
			mat = new Material[2];

			if (hName == "0") {
				mat [0] = materialsList.hairB_FadeMaterials [hairC];
				mat [1] = materialsList.hairB_Materials [hairC];
				skinRend.materials = mat;
			} else {
				skinRend.material = materialsList.hairB_FadeMaterials [hairC];

			}



		}

		//HairC==========================
		foreach (Transform child in materialsList.hairMainObjects [2].transform) {
			//print ("Foreach loop: " + child);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			//skinRend.material = materialsList.hairC_FadeMaterials [hairC];
			string oName = child.gameObject.name;
			string hName = oName.Substring (oName.Length - 1);
			//print (hName);
			mat = new Material[2];

			if (hName == "0") {
				mat [0] = materialsList.hairC_FadeMaterials [hairC];
				mat [1] = materialsList.hairC_Materials [hairC];
				skinRend.materials = mat;
			} else {
				skinRend.material = materialsList.hairC_FadeMaterials [hairC];

			}



		}
		//HairD
		foreach (Transform child in materialsList.hairMainObjects [3].transform) {
			//print ("Foreach loop: " + child);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			//skinRend.material = materialsList.hairC_FadeMaterials [hairC];
			string oName = child.gameObject.name;
			string hName = oName.Substring (oName.Length - 1);
			//print (hName);
			mat = new Material[2];

			if (hName == "0") {
				mat [0] = materialsList.hairD_FadeMaterials [hairC];
				mat [1] = materialsList.hairD_Materials [hairC];
				skinRend.materials = mat;
			} else {
				skinRend.material = materialsList.hairD_FadeMaterials [hairC];

			}



		}
		//HairE
		foreach (Transform child in materialsList.hairMainObjects [4].transform) {
			//print ("Foreach loop: " + child);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			//skinRend.material = materialsList.hairB_FadeMaterials [hairC];
			string oName = child.gameObject.name;
			string hName = oName.Substring (oName.Length - 1);
			//print (hName);
			mat = new Material[2];

			if (hName == "0") {
				mat [0] = materialsList.hairB_FadeMaterials [hairC];
				mat [1] = materialsList.hairB_Materials [hairC];
				skinRend.materials = mat;
			} else {
				skinRend.material = materialsList.hairB_FadeMaterials [hairC];

			}



		}

		//Renderer hairARendFade = materialsList.HairA_FadeObject.GetComponent<Renderer> ();
		//hairARendFade.material = materialsList.hairA_FadeMaterials [hairC];
		//===========================================
		//Hair Type



		for (int i = 0; i < materialsList.hairMainObjects.Length; i++) {
			materialsList.hairMainObjects [i].SetActive (false);
		}
		materialsList.hairMainObjects [hairT].SetActive (true);


		//Set Body Color
        
		foreach (Transform child in materialsList.BodyObject.transform) {
			//print ("Foreach loop: " + child);
			//child.gameObject.SetActive (false);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
            skinRend.material = materialsList.body_Materials[skinT];

        }
        foreach (Transform child in materialsList.LegsObject.transform) {
			//print ("Foreach loop: " + child);
			//child.gameObject.SetActive (false);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
            skinRend.material = materialsList.body_Materials[skinT];
        }
		foreach (Transform child in materialsList.faceType [face].transform) {
			//print ("Foreach loop: " + child);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
            skinRend.material = materialsList.face_Materials[skinT];
        }
		// Eyes colors
		foreach (Transform child in materialsList.eyes_Object.transform) {
			//print ("Foreach loop: " + child);
			//child.gameObject.SetActive (false);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = materialsList.eyeColors [eye];
		}

		//Jacket Color==========================
		foreach (Transform child in materialsList.TopObjects [1].transform) {
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			string oName = child.gameObject.name;
			string hName = oName.Substring (oName.Length - 1);
			mat = new Material[2];
			mat [0] = materialsList.JacketMaterials [jacketC];
			mat [1] = materialsList.ShirtMaterials [shirtC];
			skinRend.materials = mat;

		}

		// Shirt colors
		foreach (Transform child in materialsList.TopObjects [0].transform) {
			//print ("Foreach loop: " + child);
			//child.gameObject.SetActive (false);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = materialsList.ShirtMaterials [shirtC];
		}

		// Skirt colors
		foreach (Transform child in materialsList.BottomObjects [0].transform) {
			//print ("Foreach loop: " + child);
			//child.gameObject.SetActive (false);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = materialsList.SkirtMaterials [skirtC];
		}
		// Pants colors
		foreach (Transform child in materialsList.BottomObjects [1].transform) {
			//print ("Foreach loop: " + child);
			//child.gameObject.SetActive (false);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = materialsList.PantsMaterials [pantsC];
		}

		// Shoes colors
		foreach (Transform child in materialsList.shoesObject.transform) {
			//print ("Foreach loop: " + child);
			//child.gameObject.SetActive (false);
			Renderer skinRend = child.gameObject.GetComponent<Renderer> ();
			skinRend.material = materialsList.ShoesMaterials [shoesC];
		}



		//
	}

	void Start ()
	{
        //code for In Editor customize
        hairCol = Randomize(0, 4);
        eyeCol = Randomize(0, 5);
        hairTyp = Randomize(0, 4, "quick fix to stop ponytails");
        faceTyp = Randomize(0, 4);
        btmTyp = 1; //Skirts bug out
        topTyp = Randomize(0, 1);
        skinTyp = Randomize(0, 5);
        jacketCol = Randomize(0, 5);
        shirtCol = Randomize(0, 5);
        skirtCol = Randomize(0, 5);
        pantsCol = Randomize(0, 5);
        shoesCol = Randomize(0, 5);

        charCustomize (faceTyp, eyeCol, topTyp, btmTyp, hairTyp, hairCol, skinTyp, jacketCol, shirtCol, skirtCol, pantsCol, shoesCol);
	}
    private int Randomize(int start, int end, string hairType = "")
    {
        System.Random ran = new System.Random();
        int random = ran.Next(start, end);
        if(hairType.Length > 1 && random == 1)
        {
            random++;
        }
        return random;
    }
}
