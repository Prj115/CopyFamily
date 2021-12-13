/*

FamilyMaker

 */

using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace CopyFamily
{
  internal class FamilyMaker
  {
    private const string FamilyTemplatePath = "C:\\ProgramData\\Autodesk\\RVT 2021\\Family Templates\\English\\Metric Generic Model.rft";
    private Document doc;


    public FamilyMaker(Autodesk.Revit.ApplicationServices.Application app)
    {
      doc = app.NewFamilyDocument(FamilyTemplatePath);
    }


    public void GrabGeometryOf(Document srcDoc)
    {
      var geomElems = new FilteredElementCollector(srcDoc).OfClass(typeof(GenericForm)).ToElements();
      var elems = CollectLinked(geomElems);

      if (elems.Any())
      {
        using (Transaction t = new Transaction(doc, "Copy elements from source"))
        {
          t.Start();

          var cpo = new CopyPasteOptions();
          cpo.SetDuplicateTypeNamesHandler(new DuplicateTypeNameHandlerUseDestinationTypes());
          ElementTransformUtils.CopyElements(srcDoc, elems, doc, Transform.Identity, cpo);

          t.Commit();
        }
      }
    }


    private static ICollection<ElementId> CollectLinked(IList<Element> elems)
    {
      HashSet<ElementId> dependencies = new HashSet<ElementId>();

      foreach (var elem in elems)
      {
        CollectElementDependencies(elem, dependencies);
      }

      return dependencies;
    }

    private static void CollectElementDependencies(Element elem, HashSet<ElementId> dependencies)
    {
      if (dependencies.Contains(elem.Id))
        return;

      dependencies.Add(elem.Id);

      foreach (var id in elem.GetDependentElements(new ElementOwnerViewFilter(ElementId.InvalidElementId)))
      {
        try
        {
          Element e = elem.Document.GetElement(id);

          CollectElementDependencies(e, dependencies);

          if (e is Dimension dimension)
          {
            ReferenceArray refs = dimension.References;
            foreach (Reference reference in refs)
              CollectElementDependencies(e.Document.GetElement(reference.ElementId), dependencies);
          }
        }
        catch
        { }
      }
    }


    public void SaveFamily(string path)
    {
      var sao = new SaveAsOptions()
      {
        OverwriteExistingFile = true
      };
      doc.SaveAs(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "TestFam.rfa"), sao);
    }
  }


  internal class DuplicateTypeNameHandlerUseDestinationTypes : IDuplicateTypeNamesHandler
  {
    public DuplicateTypeAction OnDuplicateTypeNamesFound(DuplicateTypeNamesHandlerArgs args)
    {
      return DuplicateTypeAction.UseDestinationTypes;
    }
  }
}
