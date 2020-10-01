using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BWomanInstantiate : MonoBehaviour {
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
        //PonyTail,
        Medium,
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


	public Transform prefabObject;

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

	// Use this for initialization
	void Start () {
		Transform pref = Instantiate (prefabObject, gameObject.transform.position, gameObject.transform.rotation);
        pref.name = "woman1@sittingpose2";//"Interviewer";
        hairCol = Randomize(0, 4);//(int)hairColor;
        eyeCol = Randomize(0, 5);//(int)eyeColor;
        hairTyp = Randomize(1, 4);//(int)hairType;
        faceTyp = Randomize(0, 4);//(int)faceType;
        btmTyp = 1;//(int)bottomType;
        topTyp = Randomize(0, 1);//(int)topType;
        skinTyp = Randomize(0, 5);//(int)skinType;
		jacketCol = Randomize(0, 5);//(int)jacketColor;
        shirtCol = Randomize(0, 5);//(int)shirtColor;
        skirtCol = Randomize(0, 5);//(int)skirtColor;
        pantsCol = Randomize(0, 5);//(int)pantsColor;
        shoesCol = Randomize(0, 5);//(int)shoesColor;
        pref.gameObject.GetComponent<BWomanCustomise> ().charCustomize(faceTyp, eyeCol, topTyp, btmTyp, 1, hairCol, skinTyp, jacketCol, shirtCol, skirtCol, pantsCol, shoesCol);

	}
    private int Randomize(int start, int end)
    {
        System.Random ran = new System.Random();
        int random = ran.Next(start, end);
        return random;
    }
}
