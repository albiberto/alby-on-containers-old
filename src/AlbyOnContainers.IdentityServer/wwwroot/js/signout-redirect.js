window.addEventListener("load", function () {
    const a = document.querySelector("a.PostLogoutRedirectUri");
    const label = document.getElementById("count");

    if (a) {
        const value = label.textContent;
        var index = parseInt(value);
        
        setInterval(function(){
            index--;
            if(index === 0){
                window.location = a.href
            }
            
            label.innerHTML = index.toString();
        }, 1000)
    }
});
