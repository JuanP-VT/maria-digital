namespace MariaDigital.LocalScanner;

public static class ExtractionPrompt
{
	public const string Instructions = """
		You read photographed Spanish registration sheets for a parish catechism program
		("INSCRIPCION CATEQUESIS") and return the data as JSON that follows the provided schema.

		Rules:
		- Transcribe handwriting exactly as written. Keep Spanish spelling and accents. Do not translate, correct or invent anything.
		- If a field is blank, unreadable or missing, use null (use an empty string for "level"). Never guess.
		- Ignore text that is crossed out or scratched over.
		- Ignore the orange highlighter notes (weekday, group number, "joven") and the folio number.
		- Dates use the format yyyy-MM-dd. If the year is missing, use null.
		- Booleans are true only when the sheet clearly says yes ("si"); blank means false.
		- "cellphone": keep only the digits.
		- "healthInstitution" is one of "None", "IMSS", "ISSSTE", "Other". Use "None" when blank.
		- "financialContribution" is the amount in pesos from the receipt stub in the top-left corner, as a number (for example 350).

		Field mapping:
		- level: NIVEL
		- name: NOMBRE DEL NIÑO (A)
		- dateOfBirth: FECHA DE NACIMIENTO
		- dateAndPlaceOfBaptism: FECHA Y LUGAR DE BAUTISMO
		- dateAndPlaceOfConfirmation: FECHA Y LUGAR DE CONFIRMACIÓN
		- dateAndPlaceOfFirstCommunion: FECHA Y LUGAR DE LA 1RA. COMUNIÓN
		- fathersName: NOMBRE DEL PAPÁ
		- mothersName: NOMBRE DE LA MAMÁ
		- areParentsMarriedByChurch: ¿ESTÁN CASADOS POR LA IGLESIA LOS PAPÁS?
		- doParentsLiveTogether: ¿VIVEN JUNTOS?
		- address: DOMICILIO
		- cellphone: TEL. O CELULAR
		- email: CORREO ELECTRONICO
		- godparentName: NOMBRE DE PADRINO
		- schoolName: ESCUELA
		- schoolGrade: GRADO
		- extracurricularActivities: ACTIVIDADES EXTRAS
		- illnessesOrAllergies: ENFERMEDADES O ALERGIAS QUE PADECE
		- medicationTaken: ¿CUÁL MEDICAMENTO TOMA?
		- healthInstitution: IMSS O ISSSTE
		- authorizedPickupPerson: QUIEN RECOGERÁ AL NIÑO (A)
		- wishToServeInChoir: CORO
		- wishToServeAsAcolyte: MONAGUILLOS
		- catechistName: CATEQUISTA

		Return only the JSON object.
		""";

	public const string UserMessage = "Extract the registration sheet in this photo.";
}
