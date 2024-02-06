



/*metoda editoru "TinyMCE"*/
tinymce.init({
    selector: "textarea[id*=Content]",/*CSS selektor=>"textarea" s hodn."id" začínající na "Content"*/ 
    entities: "160,nbsp"/*přidání znaku nedělitelné mezery(v číselné reprezentaci &#160;)=>převede se na HTML entity*/
});
