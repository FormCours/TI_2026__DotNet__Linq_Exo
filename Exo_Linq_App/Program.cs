using Exo_Linq_Context;
using System.Globalization;
using static System.Collections.Specialized.BitVector32;
using static System.Net.Mime.MediaTypeNames;

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

// Version en expression avec le mot clef "into" pour enchainer sur un select
var r4_1bis = from s in context.Students
              group s by s.Section_ID into g
              select new
              {
                  Section = g.Key,
                  Max_Result = g.Max(gi => gi.Year_Result),
              };

foreach (var element in r4_1)
{
    Console.WriteLine(element);
}



Console.Clear();
Console.WriteLine("Bonus 03");
/*
Obtenir les cours du professeur ayant les meilleurs étudiants.

Afficher :
- Le nom du professeur
- La moyenne de sa section
- La liste de ses cours
*/


// - Version pour personne trop passionné qui veullent tout faire en une seul requete :D
var rb3 = context.Students
                 .GroupBy(st => st.Section_ID)
                 .Select(g => new
                 {
                     SectionId = g.Key,
                     SectionStudents = g.Select(gi => gi),
                     SectionAvg = g.Average(gi => gi.Year_Result),
                 })
                 .GroupBy(elem => elem.SectionAvg)
                 .OrderByDescending(elem => elem.Key)
                 .First()
                 .Join(
                    context.Professors,
                    sg => sg.SectionId,
                    p => p.Section_ID,
                    (sg, p) => new
                    {
                        SectionId = sg.SectionId,
                        SectionAvg = sg.SectionAvg,
                        ProfName = p.Professor_Name,
                        ProfId = p.Professor_ID
                    }
                 )
                 .GroupJoin(
                    context.Courses,
                    sgp => sgp.ProfId,
                    c => c.Professor_ID,
                    (sgp, courses) => new
                    {
                        SectionId = sgp.SectionId,
                        SectionAvg = sgp.SectionAvg,
                        ProfName = sgp.ProfName,
                        Courses = courses.Select(c => new
                        {
                            Name = c.Course_Name,
                            Ects = c.Course_Ects
                        })
                    }
                 );

// - Version attendu
var rb3_MaxAvgSection = context.Students.GroupBy(st => st.Section_ID)
                                        .Max(g => g.Average(gi => gi.Year_Result));

var rb3_BestSection = context.Students
                             .GroupBy(st => st.Section_ID)
                             .Select(g => new
                             {
                                 SectionId = g.Key,
                                 SectionStudents = g.Select(gi => gi),
                                 SectionAvg = g.Average(gi => gi.Year_Result),
                             })
                             .Where(g => g.SectionAvg == rb3_MaxAvgSection)
                             .Select(elem => new
                             {
                                 SectionAvg = elem.SectionAvg,
                                 SectionId = elem.SectionId
                             });

var rb3_Result = rb3_BestSection.Join(
                    context.Professors,
                    sg => sg.SectionId,
                    p => p.Section_ID,
                    (sg, p) => new
                    {
                        SectionId = sg.SectionId,
                        SectionAvg = sg.SectionAvg,
                        ProfName = p.Professor_Name,
                        ProfId = p.Professor_ID
                    }
                 )
                 .GroupJoin(
                    context.Courses,
                    sgp => sgp.ProfId,
                    c => c.Professor_ID,
                    (sgp, courses) => new
                    {
                        SectionId = sgp.SectionId,
                        SectionAvg = sgp.SectionAvg,
                        ProfName = sgp.ProfName,
                        Courses = courses.Select(c => new
                        {
                            Name = c.Course_Name,
                            Ects = c.Course_Ects
                        })
                    }
                 );


foreach (var elem in rb3_Result)
{
    Console.WriteLine($"{elem.ProfName} - {elem.SectionId} - {elem.SectionAvg}");
    foreach (var c in elem.Courses)
    {
        Console.WriteLine($"- {c.Name} {c.Ects}");
    }
}

// --------------------------------------------------------------------------------

Console.Clear();
Console.WriteLine("Exercice 4.7");
//  Donner, pour toutes les sections, le nom des professeurs qui en sont membres

var r4_7_V1 = context.Sections
                  .GroupJoin(
                        context.Professors,
                        s => s.Section_ID,
                        p => p.Section_ID,
                        (s, p) => new
                        {
                            SecId = s.Section_ID,
                            SecName = s.Section_Name,
                            Profs = p.Select(p => p.Professor_Name)
                        }
                  );


var r4_7_V2 = from section in context.Sections
              join prof in context.Professors on section.Section_ID equals prof.Section_ID into profs
              select new
              {
                  SecId = section.Section_ID,
                  SecName = section.Section_Name,
                  //Profs = profs.Select(p => p.Professor_Name),
                  Profs = from p in profs select p.Professor_Name
              };

var r4_7_V3 = context.Sections.LeftJoin(context.Professors,
                                        s => s.Section_ID,
                                        p => p.Section_ID,
                                        (s, p) => new
                                        {
                                            SecId = s.Section_ID,
                                            SecName = s.Section_Name,
                                            Prof = p?.Professor_Name
                                        })
                               .GroupBy(g => g.SecId)
                               .Select(g => new
                               {
                                   SecId = g.Key,
                                   SecName = g.First().SecName,
                                   Profs = g.Where(p => p.Prof is not null).Select(p => p.Prof)

                               });

var r4_7_V4 = context.Professors.GroupBy(p => p.Section_ID)
                                .RightJoin(context.Sections,
                                           gp => gp.Key,
                                           s => s.Section_ID,
                                           (gp, s) => new
                                           {
                                               SecId = s.Section_ID,
                                               SecName = s.Section_Name,
                                               Profs = gp?.Select(gpi => gpi.Professor_Name) ?? Enumerable.Empty<string>()
                                           });

foreach (var r in r4_7_V4)
{
    Console.WriteLine($"{r.SecId} - {r.SecName}");
    foreach (var r1 in r.Profs)
    {
        Console.WriteLine($"- {r1}");
    }
}

Console.Clear();
Console.WriteLine("Exercice 4.8");
// Idem que 4.7, mais seules les sections comportant au moins un professeur doivent être reprises.

var r4_8_V1 = context.Sections
                  .GroupJoin(
                        context.Professors,
                        s => s.Section_ID,
                        p => p.Section_ID,
                        (s, p) => new
                        {
                            SecId = s.Section_ID,
                            SecName = s.Section_Name,
                            Profs = p.Select(p => p.Professor_Name)
                        }
                  )
                  .Where(sp => sp.Profs.Any());

var r4_8_V2 = from s in context.Sections
              join p in context.Professors
                on s.Section_ID equals p.Section_ID
                into professors
              where professors.Any()
              select new
              {
                  SecId = s.Section_ID,
                  SecName = s.Section_Name,
                  Profs = professors.Select(p => p.Professor_Name)
              };

var r4_8_V3 = from p in context.Professors
              group p by p.Section_ID into grp
              join s in context.Sections
                on grp.Key equals s.Section_ID //on grp.First().Section_ID equals s.Section_ID
              select new
              {
                  SecId = s.Section_ID,
                  SecName = s.Section_Name,
                  Profs = grp.Select(p => p.Professor_Name)
              };


foreach (var r in r4_8_V3)
{
    Console.WriteLine($"{r.SecId} - {r.SecName}");
    foreach (var r1 in r.Profs)
    {
        Console.WriteLine($"- {r1}");
    }
}


Console.Clear();
Console.WriteLine(" ");
// Donner à chaque étudiant ayant obtenu un résultat annuel supérieur ou égal à 12
// son grade en fonction de son résultat annuel et sur base de la table grade. La liste doit être
// classée dans l’ordre alphabétique des grades attribués.
var r4_9_v1 = context.Students.Where(s => s.Year_Result >= 12)
                           .Select(st => new
                           {
                               Nom = st.Last_Name,
                               Result = st.Year_Result,
                               Grade = context.Grades.Single(g => st.Year_Result >= g.Lower_Bound && st.Year_Result <= g.Upper_Bound).GradeName
                           })
                           .OrderBy(tri => tri.Grade);

var r4_9_Martin = from st in context.Students
                  where st.Year_Result >= 12
                  from gr in context.Grades
                  where st.Year_Result >= gr.Lower_Bound && st.Year_Result <= gr.Upper_Bound
                  orderby gr.GradeName
                  select new
                  {
                      Name = st.Last_Name,
                      Result = st.Year_Result,
                      Grade = gr.GradeName
                  };

var r4_9_Phil = context.Students.Where(st => st.Year_Result >= 12)
                                .SelectMany(
                                    student => context.Grades,
                                    (st, gr) => new
                                    {
                                        st,
                                        gr
                                    })
                                .Where(elem => elem.st.Year_Result >= elem.gr.Lower_Bound
                                            && elem.st.Year_Result <= elem.gr.Upper_Bound)
                                .OrderBy(elem => elem.gr.GradeName)
                                .Select(elem => new
                                {
                                    Name = elem.st.Last_Name,
                                    Result = elem.st.Year_Result,
                                    Grade = elem.gr.GradeName
                                });

foreach (var elem in r4_9_Phil)
{
    Console.WriteLine(elem);
}

Console.Clear();
Console.WriteLine("Exercice 4.10");
//  Donner la liste des professeurs et la section à laquelle ils se rapportent ainsi que
// le(s) cour(s)(nom du cours et crédits) dont le professeur est responsable. La liste est triée
// par ordre décroissant des crédits attribués à un cours.

var r4_10 = context.Professors
                   .Join(context.Sections,
                         p => p.Section_ID,
                         s => s.Section_ID,
                         (p, s) => new
                         {
                             p.Professor_ID,
                             p.Professor_Name,
                             s.Section_Name
                         }
                   )
                   .LeftJoin(context.Courses,
                         psp => psp.Professor_ID,
                         c => c.Professor_ID,
                         ( psp, c ) => new
                         {
                             psp.Professor_Name,
                             psp.Section_Name,
                             c?.Course_Name,
                             c?.Course_Ects
                         }
                   )
                   .Select(r => new
                   {
                       ProfName = r.Professor_Name,
                       SectName = r.Section_Name,
                       CourName = r?.Course_Name,
                       Credit = r?.Course_Ects
                   })
                   .OrderByDescending(ord => ord.Credit);

foreach(var elem in r4_10)
{
    Console.WriteLine(elem);
}

Console.Clear();
Console.WriteLine("Exercice 4.11");
// Donner pour chaque professeur son id et le total des crédits ECTS
// (« ECTSTOT ») qui lui sont attribués. La liste proposée est triée par ordre décroissant de la
// somme des crédits alloués.

var r4_11 = context.Professors.GroupJoin(context.Courses,
                                          p => p.Professor_ID,
                                          c => c.Professor_ID,
                                          (prof, courses) => new
                                          {
                                              Id = prof.Professor_ID,
                                              Nom = prof.Professor_Name,
                                              Total = courses.Sum(c => c.Course_Ects),
                                              Total2 = courses.Any()? (float?)courses.Sum(c => c.Course_Ects): null 
                                          });

foreach(var l in r4_11)
{
    Console.WriteLine(l);
}