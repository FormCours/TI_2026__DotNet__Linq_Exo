using Exo_Linq_Context;
using System.Globalization;

Console.WriteLine("Exercice Linq");
Console.WriteLine("*************");

DataContext context = new DataContext();


Console.WriteLine();
Console.WriteLine(" - Bonus 01");
/*
Écrire une requête pour obtenir la liste des apprenants qui ont un résultat supérieur à la moyenne globale. 

Afficher les données suivants : 
 - Le prénom et nom concaténé
 - La date de naissance en français (Exemple : 01 janvier 1990)
 - Le résultat sur 100

Les données doivent être ordonnée comme ceux-ci : 
 - Ascendant sur le prénom
 - Descendant sur le nom
*/

// - Expression
var rb1_v1 = from s in context.Students
             where s.Year_Result > (context.Students.Average(st => st.Year_Result))
             orderby s.First_Name, s.Last_Name descending
             select new
             {
                 Nom = $"{s.First_Name} {s.Last_Name}",
                 DateNaissance = s.BirthDate.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-be")),
                 Resultat = s.Year_Result * 5
             };

// - Méthode
var rb1_v2 = context.Students
                    .Where(s => s.Year_Result > context.Students.Average(st => st.Year_Result))
                    .OrderBy(s => s.First_Name)
                    .ThenByDescending(s => s.Last_Name)
                    .Select(s => new
                    {
                        Nom = $"{s.First_Name} {s.Last_Name}",
                        DateNaissance = s.BirthDate.ToString("dd MMMM yyyy", CultureInfo.CreateSpecificCulture("fr-be")),
                        Resultat = s.Year_Result * 5
                    });

// - Affichage
foreach (var elem in rb1_v2)
{
    Console.WriteLine(elem);
}


Console.WriteLine();
Console.WriteLine("Bonus 02");
/*
Écrire une requête pour obtenir les liste des apprenants ayant obtenu la meilleur note en étant en échec (strictement inférieur à 10).

Afficher : 
 - Le prénom et l'initial de son nom concaténé
 - Le résultat
 - L'id de section

Les données doivent être ordonnée : 
 - Descendant sur l'initial du nom de famille
 - Descendant sur l'id de section
 - Ascendant sur le prénom
*/

// - Méthode
var rb2_v1 = context.Students
                    .Where(s => s.Year_Result == (context.Students.Where(s => s.Year_Result < 10).Max(s => s.Year_Result)))
                    .Select(s => new { Name = s.First_Name, Initial = s.Last_Name[0].ToString(), s.Section_ID, s.Year_Result })
                    .OrderByDescending(s => s.Initial)
                    .ThenByDescending(s => s.Section_ID)
                    .ThenBy(s => s.Name)
                    .Select(s => new { Nom = $"{s.Name} {s.Initial}", Resultat = s.Year_Result, Section = s.Section_ID });


// - Expression
var rb2_v2 = from s1 in context.Students
             where s1.Year_Result == ((from s2 in context.Students where s2.Year_Result < 10 select s2.Year_Result).Max())
             orderby s1.Last_Name[0] descending, s1.Section_ID descending, s1.First_Name
             select new { Nom = $"{s1.First_Name} {s1.Last_Name[0]}", Resultat = s1.Year_Result, Section = s1.Section_ID };

// - Affichage
foreach (var elem in rb2_v1)
{
    Console.WriteLine(elem);
}

// --------------------------------------------------------------

Console.WriteLine();
Console.WriteLine("Exercice 4.1");
// Donner pour chaque section, le résultat maximum (« Max_Result ») obtenu par les étudiants.
var r4_1 = context.Students.GroupBy(s => s.Section_ID)
                         .Select(g => new
                         {
                             Max_Result = g.Max(gi => gi.Year_Result),
                             Section = g.Key
                         });

foreach (var element in r4_1)
{
    Console.WriteLine(element);
}