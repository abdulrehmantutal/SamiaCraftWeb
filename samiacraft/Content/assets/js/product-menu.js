
filterSelection("all")
function filterSelection(c) {
  var x, i;
  x = document.getElementsByClassName("Products");
  if (c == "all") c = "";
  for (i = 0; i < x.length; i++) {
    ClassRemove(x[i], "show");
    if (x[i].className.indexOf(c) > -1) ClassAdd(x[i], "show");
  }
}

function ClassAdd(element, name) {
  var i, arr1, arr2;
  arr1 = element.className.split(" ");
  arr2 = name.split(" ");
  for (i = 0; i < arr2.length; i++) {
    if (arr1.indexOf(arr2[i]) == -1) {element.className += " " + arr2[i];}
  }
}

function ClassRemove(element, name) {
  var i, arr1, arr2;
  arr1 = element.className.split(" ");
  arr2 = name.split(" ");
  for (i = 0; i < arr2.length; i++) {
    while (arr1.indexOf(arr2[i]) > -1) {
      arr1.splice(arr1.indexOf(arr2[i]), 1);     
    }
  }
  element.className = arr1.join(" ");
}


// Add active class to the current button (highlight it)
/*var btnContainer = document.getElementById("Prod-links-cont");
var btns = btnContainer.getElementsByClassName("prod-link");

for (var i = 0; i < btns.length; i++) {
  btns[i].addEventListener("click", function(){
    var current = document.getElementsByClassName("active");
    current[0].className = current[0].className.replace("active","");
    this.className += " active";
  });
}*/

