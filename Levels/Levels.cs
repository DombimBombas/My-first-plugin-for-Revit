using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection.Emit;

namespace Levels
{
    [Transaction(TransactionMode.Manual)]
    public class Levels : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            var walls = new FilteredElementCollector(doc)
                .OfClass(typeof(Wall))
                .ToList();

            var levels = new FilteredElementCollector(doc)
                .OfClass(typeof(Level))
                .ToList();

            Level correctLevel2 = (Level)levels.FirstOrDefault(l => l.Name == "Правильный 2");
            Level correctLevel3 = (Level)levels.FirstOrDefault(l => l.Name == "Правильный 3");
            Level incorrectLevel2 = (Level)levels.FirstOrDefault(l => l.Name == "Неправильный 2");
            Level incorrectLevel3 = (Level)levels.FirstOrDefault(l => l.Name == "Неправильный 3");

            if (correctLevel2 == null || correctLevel3 == null || incorrectLevel2 == null || incorrectLevel3 == null)
            {
                TaskDialog.Show("Error", "Не удалось найти все необходимые уровни.");
            }

            using (Transaction tr = new Transaction(doc, "Check levels"))
            {
                tr.Start();

                foreach (Wall wall in walls)
                {
                    Parameter baseConstraint = wall.LookupParameter("Зависимость снизу");
                    Parameter topConstraint = wall.LookupParameter("Зависимость сверху");

                    if (baseConstraint != null && topConstraint != null)
                    {
                        if (baseConstraint.AsElementId() == incorrectLevel2.Id)
                        {
                            baseConstraint.Set(correctLevel2.Id);
                        }
                        else if (baseConstraint.AsElementId() == incorrectLevel3.Id)
                        {
                            baseConstraint.Set(correctLevel3.Id);
                        }

                        if (topConstraint.AsElementId() == incorrectLevel2.Id)
                        {
                            topConstraint.Set(correctLevel2.Id);
                        }
                        else if (topConstraint.AsElementId() == incorrectLevel3.Id)
                        {
                            topConstraint.Set(correctLevel3.Id);
                        }
                    }
                }
                doc.Delete(incorrectLevel2.Id);
                doc.Delete(incorrectLevel3.Id);

                tr.Commit();
            }
            return Result.Succeeded;
        }
    }
}
