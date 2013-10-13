using UnityEngine;
using System.Collections;

[System.Serializable]
public class CharacterCustomization {
	public Color hairColor;
	public Color eyeColor;
	public Color eyeBackgroundColor;
	public Color clothesColor;
	public Color skirtColor;
	public Color stockingsColor;
	
	public CharacterCustomization() {
		hairColor = Color.white;
		eyeColor = Color.white;
		eyeBackgroundColor = Color.white;
		clothesColor = Color.white;
		skirtColor = Color.white;
		stockingsColor = Color.white;
	}
	
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
		myObj.hairColor = stream.Read<Color>();
		myObj.eyeColor = stream.Read<Color>();
		myObj.eyeBackgroundColor = stream.Read<Color>();
		myObj.clothesColor = stream.Read<Color>();
		myObj.skirtColor = stream.Read<Color>();
		myObj.stockingsColor = stream.Read<Color>();
		return myObj;
	}
}
