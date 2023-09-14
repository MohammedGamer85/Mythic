﻿using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Reflection.Metadata;
using System.Windows.Input;
using mythos.Data;
using mythos.Services;
using mythos.UI.Services;
using mythos.Models;

namespace mythos.Features.Mod;

public class EnableDisableMods : ICommand
{
    public event EventHandler CanExecuteChanged;
    private string _path;
    private Dictionary<string, string> fileNames;

    public bool CanExecute(object parameter)
    {
        // Add your code to determine whether the command can execute or not
        int id = Convert.ToInt32(parameter);

        if (ImportedModsInfo.Mods[id] == null || ImportedModsInfo.Mods[id] == new ImportedModsItem())
        {
            Logger.Log($"Failed To Enable/Disable {ImportedModsInfo.Mods[id].Name}, " +
                $"Error: Mod[Name:{ImportedModsInfo.Mods[id].Name} Id:{ImportedModsInfo.Mods[id].Id}] Does not contain data or contain invaild data");

            MiddleMan.OpenMessageWindowFromMythos.Invoke($"Failed To Enable/Disable {ImportedModsInfo.Mods[id].Name}, " +
                $"Error: Mod[Name:{ImportedModsInfo.Mods[id].Name} Id:{ImportedModsInfo.Mods[id].Id}] Does not contain data or contain invaild data");

            return false;
        }
        else
        {
            return true;
        }
    }

    public void Execute(object parameter)
    {
        // Add your code that will be executed when the command is invoked
        try
        {
            int id = Convert.ToInt32(parameter);

            _path = Path.Combine(FilePaths.GetMythosDownloadsFolder, ImportedModsInfo.Mods[Convert.ToInt32(parameter)].Uuid);

            fileNames = JsonReaderHelper.ReadJsonFile<Dictionary<string, string>>(Path.Combine(_path, "modInfo.json"));

            if (ImportedModsInfo.Mods[id].IsLoaded == true)
                Disable(id);
            else
                Enable(id);

            JsonWriterHelper.WriteJsonFile("importedMods.json", ImportedModsInfo.Mods);
        }
        catch (Exception ex)
        {
            Logger.Log($"Failed to Excute, Exception is [{ex.ToString()}].");
            MiddleMan.OpenMessageWindowFromMythos.Invoke($"Failed to Excute, Exception is [{ex.Message}].");
        }
    }

    public void Enable(int id)
    {
        Logger.Log($"Enabling {ImportedModsInfo.Mods[id].Uuid}");
        try
        {
            if (!Directory.Exists(Path.Combine(_path, fileNames["RP"])))
                throw new Exception("Could not find RP");

            DirectoryUtilities.Copy(Path.Combine(_path, fileNames["RP"]), Path.Combine(FilePaths.GetMythsRPFolder, fileNames["RP"]), true);

            if (Directory.Exists(Path.Combine(_path, fileNames["BP"])))
                DirectoryUtilities.Copy(Path.Combine(_path, fileNames["BP"]), Path.Combine(FilePaths.GetMythsBPFolder, fileNames["BP"]), true);

            ImportedModsInfo.Mods[id].IsLoaded = true;
        }
        catch (Exception ex)
        {
            Logger.Log($"Failed To enable {ImportedModsInfo.Mods[id].Name}, Exception is [{ex.ToString()}].");
            MiddleMan.OpenMessageWindowFromMythos.Invoke($"Failed To enable {ImportedModsInfo.Mods[id].Name}, Exception is [{ex.Message}].");
        }
    }

    public void Disable(int id)
    {
        Logger.Log($"Disabling {ImportedModsInfo.Mods[id].Uuid}");
        try
        {
            if (!Directory.Exists(Path.Combine(FilePaths.GetMythsRPFolder, fileNames["RP"])))
                throw new Exception("Could not find RP");

            Directory.Delete(Path.Combine(FilePaths.GetMythsRPFolder, fileNames["RP"]), true);

            if (Directory.Exists(Path.Combine(FilePaths.GetMythsBPFolder, fileNames["BP"])))
                Directory.Delete(Path.Combine(FilePaths.GetMythsBPFolder, fileNames["BP"]), true);

            ImportedModsInfo.Mods[id].IsLoaded = false;
        }
        catch (Exception ex)
        {
            Logger.Log($"Failed To disable {ImportedModsInfo.Mods[id].Name} Exception is [{ex.ToString()}]");
            MiddleMan.OpenMessageWindowFromMythos.Invoke($"Failed To Disable {ImportedModsInfo.Mods[id].Name} Exception is [{ex.Message}]");
        }
    }
}
