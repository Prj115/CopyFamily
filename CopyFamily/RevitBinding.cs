/*

Creates new family and copy geometry form current open family


 */


using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;

namespace CopyFamily
{
  [TransactionAttribute(TransactionMode.Manual)]
  [RegenerationAttribute(RegenerationOption.Manual)]
  public sealed class RevitBinding : IExternalCommand
  {
    public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
    {
      var uiApp = commandData.Application;

      var maker = new FamilyMaker(uiApp.Application);
      maker.GrabGeometryOf(uiApp.ActiveUIDocument.Document);

      var path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TestFam.rfa");
      maker.SaveFamily(path);
      uiApp.OpenAndActivateDocument(path);

      return Result.Succeeded;
    }
  }
}
