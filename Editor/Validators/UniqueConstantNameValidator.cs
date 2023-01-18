//using Sirenix.OdinInspector.Editor.Validation;
//using System.Linq;
//using UnityEditor;
//using UnityEngine;

//[assembly: RegisterValidator(typeof(Vaflov.UniqueConstantNameValidator))]

//namespace Vaflov {
//    public class UniqueConstantNameValidator : AttributeValidator<UniqueConstantNameAttribute, string> {
//        protected override void Validate(ValidationResult result) {
//            //Debug.Log("UniqueConstantNameValidator");
//            var targetName = ValueEntry.SmartValue;
//            if (string.IsNullOrEmpty(targetName)) {
//                result.Message = "Empty constant name";
//                result.ResultType = ValidationResultType.Error;
//                return;
//            }

//            var constantTypes = TypeCache.GetTypesDerivedFrom(typeof(Constant<>))
//                .Where(type => !type.IsGenericType)
//                .ToList();

//            foreach (var constantType in constantTypes) {
//                var constantAssetGuids = AssetDatabase.FindAssets($"t: {constantType}");
//                foreach (var constantAssetGuid in constantAssetGuids) {
//                    var assetPath = AssetDatabase.GUIDToAssetPath(constantAssetGuid);
//                    var constantAsset = AssetDatabase.LoadAssetAtPath(assetPath, constantType);

//                    if (constantAsset.name == targetName) {
//                        result.Message = "Constant name taken";
//                        result.ResultType = ValidationResultType.Error;
//                        return;
//                    }
//                }
//            }
//        }
//    }
//}
