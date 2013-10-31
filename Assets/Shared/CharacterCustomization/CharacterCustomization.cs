using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterCustomization {
	public const float baseHeight = 1f;
	public const float baseVoicePitch = 1.12f;
	
	public const float heightMultiplier = 0.25f;
	public const float voicePitchMultiplier = 0.12f;
	
	public float height;
	public float voicePitch;
	public Color hairColor;
	public Color eyeColor;
	public Color eyeBackgroundColor;
	public Color clothesColor;
	public Color skirtColor;
	public Color stockingsColor;
	
	// Constructor
	public CharacterCustomization() {
		height = 0.5f;
		voicePitch = 0.5f;
		hairColor = Color.white;
		eyeColor = Color.white;
		eyeBackgroundColor = Color.white;
		clothesColor = Color.white;
		skirtColor = Color.white;
		stockingsColor = Color.white;
	}
	
	// Update materials
	public void UpdateMaterials(Transform previewModel) {
		if(previewModel == null)
			return;
		
		var hairMaterial = previewModel.FindChild("Hair").renderer.material;
		var eyesMaterial = previewModel.FindChild("Eyes").renderer.material;
		var clothesMaterial = previewModel.FindChild("Clothes").renderer.materials[0];
		var skirtMaterial = previewModel.FindChild("Clothes").renderer.materials[1];
		var stockingsMaterial = previewModel.FindChild("Body").renderer.materials[1];
		
		hairMaterial.color = this.hairColor;
		eyesMaterial.color = this.eyeColor;
		eyesMaterial.SetColor("_BackgroundColor", this.eyeBackgroundColor);
		clothesMaterial.color = this.clothesColor;
		skirtMaterial.color = this.skirtColor;
		stockingsMaterial.color = this.stockingsColor;
	}
	
	// Scale vector
	public Vector3 scaleVector {
		get {
			float newScale = CharacterCustomization.baseHeight + (height - 0.5f) * CharacterCustomization.heightMultiplier;
			return new Vector3(newScale, newScale, newScale);
		}
	}
	
	// Final voice pitch
	public float finalVoicePitch {
		get {
			return CharacterCustomization.baseVoicePitch + (voicePitch - 0.5f) * CharacterCustomization.voicePitchMultiplier;
		}
	}
	
	// Writer
	public static void JsonSerializer(Jboy.JsonWriter writer, object instance) {
		GenericSerializer.WriteJSONClassInstance<CharacterCustomization>(writer, (CharacterCustomization)instance);
	}
	
	// Reader
	public static object JsonDeserializer(Jboy.JsonReader reader) {
		return GenericSerializer.ReadJSONClassInstance<CharacterCustomization>(reader);
	}
	
	// BitStream Writer
	public static void WriteToBitStream(uLink.BitStream stream, object val, params object[] args) {
		var myObj = (CharacterCustomization)val;
		stream.Write<float>(myObj.height);
		stream.Write<float>(myObj.voicePitch);
		stream.Write<Color>(myObj.hairColor);
		stream.Write<Color>(myObj.eyeColor);
		stream.Write<Color>(myObj.eyeBackgroundColor);
		stream.Write<Color>(myObj.clothesColor);
		stream.Write<Color>(myObj.skirtColor);
		stream.Write<Color>(myObj.stockingsColor);
	}
	
	// BitStream Reader
	public static object ReadFromBitStream(uLink.BitStream stream, params object[] args) {
		var myObj = new CharacterCustomization();
		myObj.height = stream.Read<float>();
		myObj.voicePitch = stream.Read<float>();
		myObj.hairColor = stream.Read<Color>();
		myObj.eyeColor = stream.Read<Color>();
		myObj.eyeBackgroundColor = stream.Read<Color>();
		myObj.clothesColor = stream.Read<Color>();
		myObj.skirtColor = stream.Read<Color>();
		myObj.stockingsColor = stream.Read<Color>();
		return myObj;
	}
}
