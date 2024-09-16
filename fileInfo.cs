
namespace fileInfo {
  public class PartialFile {
  public string name;

  public string content;
  public string? parentFile;
  public bool eof;
  public int charCant;
  public string creationDate;
  public string modifDate;
  public string deletionDate;

  public PartialFile(
    string name,
    string content,
    string? parent,
    int charCant,
    string creationDate
  ) {
    this.name = name;
    this.content = content;
    this.parentFile = parent;
    this.eof = false;
    this.charCant = charCant;
    this.creationDate = creationDate;
    this.modifDate = creationDate;
    this.deletionDate = "";
  }

  public string String() {
    string prov = " modificación";
    string selectedDate = modifDate;
    if (eof) {
      prov = " eliminación";
      selectedDate = deletionDate;
    }
    string text = "";
    text += "----------------------------\n" +
    $"Nombre: {name}\n" + $"Tamaño en caracteres: {charCant}\n" + 
    $"Fecha de creación: {creationDate}\n" + $"Fecha de{prov}: {selectedDate}\n" +
    "---------------------------\n" + content + "\n-------------------------";
    return text;
  }

}
}
