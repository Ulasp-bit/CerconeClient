﻿using CerconeClient.Dtos;
using CerconeClient.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows.Forms;

namespace CerconeClient.Services
{
    public class CerconeData
    {
        public void UpdatePsjData(string path)
        {
            using HttpClient client = new HttpClient();
            string[] sheets = new string[2]
            { 
            "https://sheets.googleapis.com/v4/spreadsheets/1RY3WbHMMKznrKheDQYYrbnCIfk9ukOigpAR84cuUHIk/values/ROSTER?key=AIzaSyDbgaqwB_8Yt5FQdZdYg7_iLv2__1mmRtc", 
            "https://sheets.googleapis.com/v4/spreadsheets/1RY3WbHMMKznrKheDQYYrbnCIfk9ukOigpAR84cuUHIk/values/GRIMORIO?key=AIzaSyDbgaqwB_8Yt5FQdZdYg7_iLv2__1mmRtc" 
            };

            HttpResponseMessage response = client.GetAsync(sheets[0]).Result;
            HttpResponseMessage grimoireResponse = client.GetAsync(sheets[1]).Result;

            var rosterData = response.Content.ReadAsStringAsync().Result;
            var grimoireData = grimoireResponse.Content.ReadAsStringAsync().Result;
            var dto = JsonSerializer.Deserialize<GoogleSheetData>(rosterData, new JsonSerializerOptions() { });
            var grimoireDto = JsonSerializer.Deserialize<GoogleSheetData>(grimoireData, new JsonSerializerOptions() { });
            CreateFilesPsj(dto!, grimoireDto!, path);
        }
        private void CreateFilesPsj(GoogleSheetData dto, GoogleSheetData grimoireDto, string path)
        {
            var pjs = new List<PjsInfo>();
            var grimoire = new List<GrimoireInfo>();
            dto.values.Remove(dto.values.First());
            dto.values.Remove(dto.values.First());
            grimoireDto.values.Remove(grimoireDto.values.First());

            foreach (var rows in dto.values)
            {
                if (rows[0] == "") continue;
                var pj = PjHelper.GetPjInfo(rows);
                if (pj != null) pjs.Add(pj);
            }

            foreach (var rows in grimoireDto.values)
            {   
                if (rows == null || rows[0] == "") continue;
                try{
                    var grimoireInfo = GrimoireHelper.GetGrimoireInfo(rows);
                    if (grimoireInfo != null) grimoire.Add(grimoireInfo);
                }
                catch(Exception e)
                {
                    MessageBox.Show($"Error al procesar la fila: {string.Join(", ", rows)}. Error: {e.Message}");
                }
            }

            BuildLuaData(pjs, grimoire, path);
        }

        private void BuildLuaData(List<PjsInfo> pjs, List<GrimoireInfo> grimoire, string path)
        {
            var strindToSave = ConvertPjsToLua(pjs);
            var strindToSave2 = ConvertGrimToLua(grimoire);
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"CerconePjData.lua"), strindToSave);
            File.WriteAllText(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty, @"CerconeGrimData.lua"), strindToSave2);
        }
        private string ConvertPjsToLua(List<PjsInfo> pjs)
        {
            StringBuilder luaStringBuilder = new StringBuilder();

            luaStringBuilder.AppendLine("CerconePjData = {");
            foreach (var pj in pjs)
            {
                luaStringBuilder.AppendLine($"    {{");
                luaStringBuilder.AppendLine($"        Personaje=\"{pj.Personaje}\",");
                luaStringBuilder.AppendLine($"        ID=\"{pj.ID}\",");
                luaStringBuilder.AppendLine("        DataGeneral={");
                luaStringBuilder.AppendLine($"            Raza=\"{pj.DataGeneral.Raza}\",");
                luaStringBuilder.AppendLine($"            Clase=\"{pj.DataGeneral.Clase}\",");
                luaStringBuilder.AppendLine($"            Nacimiento=\"{pj.DataGeneral.Nacimiento}\",");
                luaStringBuilder.AppendLine($"            FechaConvercion=\"{pj.DataGeneral.FechaConvercion}\",");
                luaStringBuilder.AppendLine($"            Sire=\"{pj.DataGeneral.Sire}\",");
                luaStringBuilder.AppendLine($"            Armadura=\"{pj.DataGeneral.Armadura}\",");
                luaStringBuilder.AppendLine($"            Rango=\"{pj.DataGeneral.Rango}\",");
                luaStringBuilder.AppendLine($"            Orden=\"{pj.DataGeneral.Orden}\",");
                luaStringBuilder.AppendLine($"            Arma=\"{pj.DataGeneral.Arma}\",");
                luaStringBuilder.AppendLine($"            Profesion=\"{pj.DataGeneral.Profesion}\"");
                luaStringBuilder.AppendLine("        },");
                luaStringBuilder.AppendLine($"        ProfLevel={pj.ProfLevel},");
                luaStringBuilder.AppendLine($"        HP={pj.HP},");
                luaStringBuilder.AppendLine($"        Magicka={pj.Magicka},");
                luaStringBuilder.AppendLine($"        Ataque=\"{pj.Ataque}\",");
                luaStringBuilder.AppendLine($"        Defensa={pj.Defensa},");
                luaStringBuilder.AppendLine("        Meritos={");
                luaStringBuilder.AppendLine($"            PorPorCampana=\"{pj.Meritos.PorPorCampana}\",");
                luaStringBuilder.AppendLine($"            PorTaberna=\"{pj.Meritos.PorTaberna}\",");
                luaStringBuilder.AppendLine($"            PorMisiones=\"{pj.Meritos.PorMisiones}\",");
                luaStringBuilder.AppendLine($"            Otros=\"{pj.Meritos.Otros}\",");
                luaStringBuilder.AppendLine($"            MeritosGastados=\"{pj.Meritos.MeritosGastados}\",");
                luaStringBuilder.AppendLine($"            TotalMeritos=\"{pj.Meritos.TotalMeritos}\",");
                luaStringBuilder.AppendLine($"            Misiones=\"{pj.Meritos.Misiones}\"");
                luaStringBuilder.AppendLine("        },");
                luaStringBuilder.AppendLine("        HabilidadesCombatientes={");
                luaStringBuilder.AppendLine($"            LinajeCercone={pj.HabilidadesCombatientes.LinajeCercone},");
                luaStringBuilder.AppendLine($"            ArteDeGuerra={pj.HabilidadesCombatientes.ArteDeGuerra},");
                luaStringBuilder.AppendLine($"            LeccionesClase={pj.HabilidadesCombatientes.LeccionesClase}");
                luaStringBuilder.AppendLine("        },");
                luaStringBuilder.AppendLine("        HabilidadesNOCombatientes={");
                luaStringBuilder.AppendLine($"            Exploracion={pj.HabilidadesNOCombatientes.Exploracion},");
                luaStringBuilder.AppendLine($"            Investigacion={pj.HabilidadesNOCombatientes.Investigacion},");
                luaStringBuilder.AppendLine($"            InutilizarM={pj.HabilidadesNOCombatientes.InutilizarM},");
                luaStringBuilder.AppendLine($"            Sigilo={pj.HabilidadesNOCombatientes.Sigilo},");
                luaStringBuilder.AppendLine($"            Persuacion={pj.HabilidadesNOCombatientes.Persuacion},");
                luaStringBuilder.AppendLine($"            Intimidacion={pj.HabilidadesNOCombatientes.Intimidacion},");
                // luaStringBuilder.AppendLine($"            Engano={pj.HabilidadesNOCombatientes.Engano},");
                luaStringBuilder.AppendLine($"            Voluntad={pj.HabilidadesNOCombatientes.Voluntad},");
                luaStringBuilder.AppendLine($"            Percepcion={pj.HabilidadesNOCombatientes.Percepcion},");
                luaStringBuilder.AppendLine($"            Fuerza={pj.HabilidadesNOCombatientes.Fuerza}");
                luaStringBuilder.AppendLine("        }");
                luaStringBuilder.AppendLine("    },");
            }

            luaStringBuilder.AppendLine("}");

            return luaStringBuilder.ToString();
        }

        private static string ConvertGrimToLua(List<GrimoireInfo> grimoire)
        {
            StringBuilder luaStringBuilder = new StringBuilder();

            luaStringBuilder.AppendLine("CerconeGrimoireData = {");
            foreach (var data in grimoire)
            {
                luaStringBuilder.AppendLine($"    {{");
                luaStringBuilder.AppendLine($"        Z=\"{data.Z?.Replace("\n", " ") ?? string.Empty}\",");
                luaStringBuilder.AppendLine($"        Nombre=\"{data.NombreClase?.Replace("\n", " ") ?? string.Empty}\",");
                luaStringBuilder.AppendLine($"        NombreRama=\"{data.NombreRama?.Replace("\n", " ") ?? string.Empty}\",");
                luaStringBuilder.AppendLine($"        Orden=\"{data.Orden?.Replace("\n", " ") ?? string.Empty}\",");
                luaStringBuilder.AppendLine($"        Descripcion=\"{data.Descripcion?.Replace("\n", "\\n") ?? string.Empty}\",");
                luaStringBuilder.AppendLine($"        Valores=\"{data.Valores?.Replace("\n", "\\n") ?? string.Empty}\",");
                luaStringBuilder.AppendLine("    },");
            }

            luaStringBuilder.AppendLine("}");

            return luaStringBuilder.ToString();
        }
    }
}
