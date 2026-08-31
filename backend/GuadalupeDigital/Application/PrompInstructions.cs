namespace GuadalupeDigital.Application
{
    public static class PromptInstructions
    {
        public static readonly string ExtractValuesFromRegistrationForm = """
            You are an expert OCR and Vision Data Extraction Model. Analyze the provided image of the "INSCRIPCIÓN CATEQUESIS" form and extract the text fields into a valid JSON object strictly adhering to the schema defined below.

            ### Output Requirements:
            1. Return ONLY the JSON object. Do not include markdown code blocks, intro text, or explanations.
            2. Standardize all dates to ISO-8601 format (YYYY-MM-DD) when possible, or extract the raw string if incomplete.
            3. Map checkboxes, radio options, or explicit Yes/No fields strictly to boolean values (`true` or `false`).
            4. If a field is empty, blank, or missing in the image, assign its value as `null`.
            5. Map "IMSS O ISSSTE" to the corresponding integer enum:
               - 0 = None
               - 1 = IMSS
               - 2 = ISSSTE
               - 3 = Other

            ### Target JSON Schema (`CatechesisEnrollment`):

            {
              "level": "string",
              "name": "string",
              "dateOfBirth": "string | null",
              "dateAndPlaceOfBaptism": "string | null",
              "dateAndPlaceOfConfirmation": "string | null",
              "dateAndPlaceOfFirstCommunion": "string | null",
              "fathersName": "string | null",
              "mothersName": "string | null",
              "areParentsMarriedByChurch": "boolean",
              "doParentsLiveTogether": "boolean",
              "address": "string | null",
              "cellphone": "string | null",
              "email": "string | null",
              "godparentName": "string | null",
              "schoolName": "string | null",
              "schoolGrade": "string | null",
              "extracurricularActivities": "string | null",
              "illnessesOrAllergies": "string | null",
              "medicationTaken": "string | null",
              "healthInstitution": "integer | null",
              "authorizedPickupPerson": "string | null",
              "wishToServeInChoir": "boolean",
              "wishToServeAsAcolyte": "boolean",
              "financialContribution": "number | null",
              "catechistName": "string | null"
            }
            """;
    }
}