using Newtonsoft.Json;
using fileInfo;
using System.Text.RegularExpressions;
using System.Security.AccessControl;
using Newtonsoft.Json.Serialization;
using System.Reflection.Metadata;

string currentDir = Directory.GetCurrentDirectory();
string thisDay = DateTime.Today.ToString("d");

void write(PartialFile file, string dir) {
  string jsonString = JsonConvert.SerializeObject(file);
  File.WriteAllText(dir, jsonString);
}

PartialFile? read(string dir) {
  try {
    string ruta = currentDir + dir;
    string jsonFromFile = File.ReadAllText(ruta);
    return JsonConvert.DeserializeObject<PartialFile>(jsonFromFile)!;
  } catch(Exception) {
    return null;
  } 
}

void append(string name) {
  File.AppendAllText(currentDir + "/filelist.txt", name + Environment.NewLine);
}

List<string> separateText(string text) {
  List<string> separationList = new List<string>();
  int charNum = text.Length;
  int slicesNum = charNum / 20;
  int lastSliceNum = charNum % 20;
  for (int i = 0; i < slicesNum; i++) {
    string slice = "";
    for (int j = i * 20; j < 20 * (i + 1); j++) {
      slice += text[j];
    }
    separationList.Add(slice);
  }
  string lastSlice = "";
  for (int k = charNum - lastSliceNum; k < charNum; k++) {
    lastSlice += text[k];
  }
  separationList.Add(lastSlice);
  return separationList;
}

void generateJsonGroup(List<string> strings, string name, bool modify) {
  if (!modify) {
    append(name);
  }
  string? parentDir = null;
  for (int i = 0; i < strings.Count; i++) {
    if (i == 0) {
      PartialFile newFile = new PartialFile(name, strings[i], parentDir, strings[i].Length, thisDay.ToString());
      parentDir = currentDir + $"/jsonfiles/{name}.json";
      write(newFile, parentDir);
    } else {
      PartialFile newFile = new PartialFile(name + i.ToString(), strings[i], parentDir, strings[i].Length, thisDay.ToString());
      parentDir = currentDir + $"/partialfiles/{name + i.ToString()}.json";
      write(newFile, currentDir + $"/partialfiles/{name + i.ToString()}.json");
    }
  }
}

PartialFile? openFile(string name) {
  string content = "";
  PartialFile? fileData = read($"/jsonfiles/{name}.json");
  if (fileData != null) {
    content += fileData.content;
    int i = 0;
    while (true) {
    i++;
    PartialFile? currentFile = read($"/partialfiles/{name}{i}.json");
    if (currentFile != null) {
      content += currentFile.content;
    } else break;
    }
  } else return null;
  return new PartialFile(name, content, fileData.parentFile, 
  content.Count(), fileData.creationDate);
}

void showList(bool eof) {
  string param = "";
  if (eof) {
    param = " en la papelera ";
  }
  Console.WriteLine($"\nListado de archivos de texto{param}: ");
  string fileNames = File.ReadAllText($"{currentDir}/filelist.txt");
  string[] fileNamesList = Regex.Split(fileNames, "\r\n|\r|\n");
  int count = 0;
  if (!string.IsNullOrEmpty(fileNames)) {
    foreach (string name in fileNamesList) {
      if (name != "") {
        PartialFile? file = read($"/jsonfiles/{name}.json");
        if (eof) {
          if (file!.eof) {
            count++;
            Console.WriteLine(count.ToString() + ". " + name);
          }
        } else {
          if (!file!.eof) {
            count++;
            Console.WriteLine(count.ToString() + ". " + name);
          }
        }
      }
  }
  }
}

void eofModify(bool op, string dir, PartialFile file) {
  file.eof = op;
  file.modifDate = thisDay;
  if (op) {
    file.deletionDate = thisDay;
  } else {
    file.deletionDate = "";
  }
  write(file, dir);
}

// true: eliminar, false: recuperar
void deleteOrRecover(string name, bool op) {
  string dir = $"/jsonfiles/{name}.json";
  PartialFile? fileData = read(dir);
  if (fileData != null) {
    eofModify(op, currentDir + dir, fileData);
    int i = 0;
    while (true) {
      i++;
      dir = $"/partialfiles/{name}{i}.json";
      PartialFile? currentFile = read(dir);
      if (currentFile != null) {
        eofModify(op, currentDir + dir, currentFile);
      } else break;
    }
  }
}

List<string> getDeletedFilesList() {
  List<string> deleted = new List<string>();
  string fileNames = File.ReadAllText($"{currentDir}/filelist.txt");
  string[] fileNamesList = Regex.Split(fileNames, "\r\n|\r|\n");
  foreach (string name in fileNamesList) {
    PartialFile? file = read($"/jsonfiles/{name}.json");
    if (file != null) {
      if (file.eof) {
        deleted.Add(name);
      }
    }
  }
  return deleted;
}

void removeOld(string name) {
  string dir = currentDir + $"/jsonfiles/{name}.json";
  if (File.Exists(dir)) {
    File.Delete(dir);
    int count = 0;
    while (true) {
      count++;
      dir = currentDir + $"/partialfiles/{name}{count}.json";
      if (File.Exists(dir)) {
        File.Delete(dir);
      } else {
        break;
      }
    }
  }
}

void modifyFile(string name, List<string> strings) {
  string dir = currentDir + $"/jsonfiles/{name}.json";
  PartialFile? oldFile = read($"/jsonfiles/{name}.json");
  if (File.Exists(dir)) { 
    removeOld(name);
    generateJsonGroup(strings, name, true);
    string creationDate = oldFile!.creationDate;
    PartialFile? file = read($"/jsonfiles/{name}.json");
    file!.creationDate = creationDate;
    write(file, dir);
    int count = 0;
    while (true) {
      count++;
      dir = currentDir + $"/partialfiles/{name}{count}.json";
      PartialFile? currentFile = read(dir); 
      if (currentFile != null) {
        currentFile!.creationDate = creationDate;
        write(currentFile, currentDir + $"/partialfiles/{name}{count}.json");
      } else break;
    }
  } else {
    Console.WriteLine("No se ha encontrado el archivo");
  }

}

void main() {

  while (true) {
    Console.WriteLine("Bienvenido al sistema de archivos FAT: ");
    Console.WriteLine("1 - Crear un archivo de texto");
    Console.WriteLine("2 - Listar archivos creados");
    Console.WriteLine("3 - Abrir un archivo");
    Console.WriteLine("4 - Modificar un archivo");
    Console.WriteLine("5 - Eliminar un archivo");
    Console.WriteLine("6 - Recuperar un archivo");
    Console.WriteLine("x - SALIR");

    string sel = Console.ReadLine()!;
    Console.WriteLine(sel);
    if (sel == "1") {
      Console.WriteLine("Ingrese el nombre del nuevo archivo: ");
      string name = Console.ReadLine()!;
      Console.WriteLine("Escriba el contenido del nuevo archivo: ");
      string content = Console.ReadLine()!;
      generateJsonGroup(separateText(content), name, false);

    } else if (sel == "2") {
      showList(false);
      
    } else if (sel == "3") {
      showList(false);
      string fileNames = File.ReadAllText($"{currentDir}/filelist.txt");
      string[] fileNamesList = Regex.Split(fileNames, "\r\n|\r|\n");
      Console.WriteLine("Escriba el número del archivo a abrir: ");
      int num = Convert.ToInt32(Console.ReadLine()!);
      PartialFile? file = openFile(fileNamesList[num - 1]);
      if (file != null) {
        if (!file.eof) {
          Console.WriteLine(file.String());
        } else {
          Console.WriteLine("El archivo no ha sido encontrado.");
        }
      } else {
        Console.WriteLine("El archivo no ha sido encontrado.");
      }
      
    } else if (sel == "4") {
      showList(false);
      string fileNames = File.ReadAllText($"{currentDir}/filelist.txt");
      string[] fileNamesList = Regex.Split(fileNames, "\r\n|\r|\n");
      Console.WriteLine("Escriba el número del archivo a modificar: ");
      int num = Convert.ToInt32(Console.ReadLine()!);
      string fileDir = $"/jsonfiles/{fileNamesList[num - 1]}.json";
      PartialFile? file = read(fileDir);
      if (file != null) {
        if (!file.eof) {
          Console.WriteLine(openFile(fileNamesList[num - 1])!.String());
          Console.WriteLine("Ingrese el nuevo texto del archivo: ");
          string newText = Console.ReadLine()!;
          Console.WriteLine("¿Desea guardar los cambios? (s/n): ");
          string op = Console.ReadLine()!;
          if (op == "s") {
            modifyFile(fileNamesList[num - 1], separateText(newText));
          }
        } else { Console.WriteLine("El archivo no ha sido encontrado."); }
      } else {
        Console.WriteLine("El archivo no ha sido encontrado.");
      }
    } else if (sel == "5") {
      showList(false);
      string fileNames = File.ReadAllText($"{currentDir}/filelist.txt");
      string[] fileNamesList = Regex.Split(fileNames, "\r\n|\r|\n");
      Console.WriteLine("Escriba el número del archivo a eliminar: ");
      int num = Convert.ToInt32(Console.ReadLine()!);
      string fileDir = $"/jsonfiles/{fileNamesList[num - 1]}.json";
      PartialFile? file = read(fileDir);
      if (file != null) {
        if (!file.eof) {
          Console.WriteLine(openFile(fileNamesList[num - 1])!.String());
          Console.WriteLine("¿Desea eliminar el archivo? (s/n) : ");
          string op = Console.ReadLine()!;
          if (op == "s") {
            deleteOrRecover(file.name, true);
          }
        } else { Console.WriteLine("El archivo no ha sido encontrado."); }
      } else {
        Console.WriteLine("El archivo no ha sido encontrado.");
      }

    } else if (sel == "6") {
      showList(true);
      List<string> fileNamesList = getDeletedFilesList();
      Console.WriteLine("Escriba el número del archivo a recuperar: ");
      int num = Convert.ToInt32(Console.ReadLine()!);
      string fileDir = $"/jsonfiles/{fileNamesList[num - 1]}.json";
      PartialFile? file = read(fileDir);
      if (file != null) {
        if (file.eof) {
          PartialFile open = openFile(fileNamesList[num - 1])!;
          open.eof = true;
          open.deletionDate = file.deletionDate;
          Console.WriteLine(open.String());
          Console.WriteLine("¿Desea recuperar el archivo? (s/n) : ");
          string op = Console.ReadLine()!;
          if (op == "s") {
            deleteOrRecover(file.name, false);
          }
        } else { Console.WriteLine("El archivo no ha sido encontrado en la papelera."); }
      } else {
        Console.WriteLine("El archivo no ha sido encontrado.");
      }
    } else if (sel == "x") {
      break;
    }
  }
}

main();