﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetCore2Blockly.JavascriptGeneration
{
    /// <summary>
    /// generates js for seeing blocks in toolbox
    /// </summary>
    public class BlocklyToolBoxJSGenerator
    {
        private string typeNameNoGeneric(Type type)
        {
            var t = Nullable.GetUnderlyingType(type) ?? type;
            return t.Name;
        }
        /// <summary>
        /// Generates the blockly tool box value.
        /// </summary>
        /// <param name="types">The types.</param>
        /// <returns></returns>
        public string GenerateBlocklyToolBoxValue(Type[] types)
        {
            string blockText = "";
            var globalVars = "var glbVar=function(workspace){";
            foreach (var type in types)
            {

                var typeName = typeNameNoGeneric(type);
                var newTypeName = type.TranslateToNewTypeName(); 

                globalVars += $"workspace.createVariable('var_{typeName}', '{newTypeName}');";
                blockText += $@"{Environment.NewLine}
                                var blockText_{typeName} = '<block type=""{newTypeName}"">';
                              ";
                blockText = GenerateToolBoxCodeForAllPropertiesOfAType(blockText, type);


                blockText += $@"blockText_{typeName} += '</block>';
                                block_{typeName} = Blockly.Xml.textToDom(blockText_{typeName});
                                xmlList.push(block_{typeName});
                                var block_{typeName}Set='<block type=""variables_set""><field name=""VAR"">var_{typeName}</field></block>';
                                block_{typeName}Set = Blockly.Xml.textToDom(block_{typeName}Set);
                                xmlList.push(block_{typeName}Set);
                                ";

            }

            var strDef = $@"
                         var registerValues = function() {{
                                var xmlList = [];
                                {blockText}
                
                        return xmlList;
              }};  ";

            globalVars += "}";
            strDef += globalVars;
            return strDef;

        }

        /// <summary>
        /// Generates tool box code for all properties of a type.
        /// </summary>
        /// <param name="blockText">The block text.</param>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public string GenerateToolBoxCodeForAllPropertiesOfAType(string blockText, Type type)
        {
            var validProperties = type.GetProperties().Where(prop => prop.GetSetMethod() != null);

            foreach (var property in type.GetProperties())
            {
                var propertyType = property.PropertyType;
                if (!propertyType.ConvertibleToBlocklyType())
                    continue;
                

                blockText += createBlockShadowDef(property.Name, propertyType.TranslateToBlocklyBlocksType());

                blockText += $"blockText_{type.Name} += blockTextLocalSiteFunctions;";
            }

            return blockText;
        }

        string createBlockShadowDef(string name, string blockShadowType)
        {
            return $@"{Environment.NewLine}
                      var blockTextLocalSiteFunctions = '<value name=""val_{name}""><shadow type=""{blockShadowType}""></shadow></value>';
                      ";


        }

    }
}
