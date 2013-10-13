using System.Collections;

public static class StringExtensions {
	public static string Capitalize(this string input) {
		if(input.Length > 0) {
			return System.Char.ToUpper(input[0]) + input.Substring(1);
		}
		
		return input;
	}
	
	public static string PrettifyPlayerName(this string playerNameRequest) {
		var parts = playerNameRequest.Replace("  ", " ").ToLower().Split(' ');
		for(int i = 0; i < parts.Length; i++) {
			parts[i] = parts[i].Trim().Capitalize();
		}
		return string.Join(" ", parts);
	}
}