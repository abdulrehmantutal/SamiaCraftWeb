$(document).ready(function () {

    ShowText();
    topheadcart();
    cartitem();
    GetWishListItems();
    StockActiveColor();
    headertext();

    //CountDownTimer();
    var Currency;
    var Tags;
    var ShippingCharges;
    var Color;
    var Category;

    $(".addItemLS").click(function () {
        cartitem();
        topheadcart();
    });

    // Gift modal handling
    $('.buy-now.add-to-cart').on('click', function() {
        var itemId = $(this).data('id');
        var itemTitle = $(this).data('title');
        var qty = parseInt($('#cart_quantity').val()) || 1;
        
        // Store current item ID in hidden field
        $('#item_hidden_id').val(itemId);
        
        // Clear previous selections
        $('.gift-checkbox').prop('checked', false);
        
        // Show the gift modal
        $('#giftModal').modal('show');
        
        // Set up add gift button handler
        $('#giftModal .add-to-gift').off('click').on('click', function() {
            AddCartWithGifts(itemId, itemTitle, qty);
        });
    });

    // If close modal without adding gifts, still add the item
    $('#giftModal .close, #giftModal [data-dismiss="modal"]').on('click', function() {
        // This will let the modal close naturally
    });
});

//setting
function headertext() {

    $.ajax({
        type: "GET",
        url: '/Home/GetSetting',
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (data) {

            $('#TopHeaderText').html(data.TopHeaderText);
            if (data.Facebook != 1 || data.Facebook == null) {
                $('#facebook').addClass('d-none');
            }
            else {
                $("#facebook a").attr("href", data.FacebookUrl);
            }
            if (data.Instagram != 1 || data.Instagram == null) {
                $('#instagram').addClass('d-none');
            }
            else {
                $("#instagram a").attr("href", data.InstagramUrl);
            }
            if (data.Twitter != 1 || data.Twitter == null) {
                $('#twitter').addClass('d-none');
            }
            else {
                $("#twitter a").attr("href", data.TwitterUrl);
            }
            if (data.ShopUrl == null || data.ShopUrl == "") {
                $('.Shop-Now-Button').attr("href", "/shop/shop?MinPrice=RS0&MaxPrice=RS50000&SortID=0");
            }
            else {
                $('.Shop-Now-Button').attr("href", data.ShopUrl);
            }
            
            var lstNotification = data.NotificationsList;
            var index = Math.floor(Math.random() * lstNotification.length);
            var _notificationls = sessionStorage.getItem("_notification");
            _notificationls = _notificationls == null ? '' : _notificationls;
            if (_notificationls == '' && lstNotification.length > 0) {
                $('#staticBackdrop').modal('show');
                sessionStorage.setItem("_notification", "true");


                $('#modalTitle').html(lstNotification[index].Title);
                $('#modalDescription').html(lstNotification[index].Description);
                $('#modalButton').attr('href',lstNotification[index].ButtonURL);
                if (lstNotification[index].Image == '' || lstNotification[index].Image == null) {
                    $('#modalImg').addClass("hide");
                }
                $('#modalImg').attr('src',"https://retail.premium-pos.com"+ lstNotification[index].Image)
            }
        },
        error: function (xhr, textStatus, errorThrown) {
            //alert(xhr, textStatus, errorThrown);
        }
    });
}

//Gift
var arrGift = [];
function addgift() {

    var getgiftLSarr = getgiftLS();
    if (getgiftLSarr != null) {
        for (var i = 0; i < arrGift.length; i++) {
            getgiftLSarr.push({
                ItemID: arrGift[i].ItemID,
                GiftID: arrGift[i].GiftID,
                Title: arrGift[i].Title,
                Image: arrGift[i].Image,
                DisplayPrice: arrGift[i].DisplayPrice,
                DiscountedPrice: arrGift[i].DiscountedPrice,
                Key: arrGift[i].Key,
                ItemKey: parseInt($('#hdnItemKey').val())

            });

            setgiftLS(getgiftLSarr);
        }
    } else { setgiftLS(arrGift); }
    $('#gift').modal('toggle');

}
function setgiftLS(arr) {
    var getgiftItem = localStorage.getItem("_giftitems");
    if (getgiftItem != null) {
        localStorage.setItem("_giftitems", JSON.stringify(arr));
    }
}
function getgiftLS() {
    var getgiftItem = localStorage.getItem("_giftitems");
    if (getgiftItem != null && getgiftItem != "")
        return JSON.parse(getgiftItem);
    else
        return JSON.parse("[]");
}
function addGiftItem(checkboxElem, ItemID, GiftID, Title, Image, DisplayPrice, DiscountedPrice) {
    //addgift();
    if (checkboxElem.checked) {
        arrGift.push({ ItemID: ItemID, GiftID: GiftID, Title: Title, Image: Image, DisplayPrice: DisplayPrice, DiscountedPrice: DiscountedPrice, Key: Math.floor((Math.random() * 1000) + 1) });
    } else {
        arrGift.splice(checkboxElem, 1);
    }
}

//toast
function toast(res, condition) {

    if (condition == 1) {
        $('.toast-body').html(res);
        $('.toast-head-text').html('Success').addClass(' text-success');
        $('.toast').addClass(' bg-green text-success');
        $('.toast').toast({ delay: 3000 }).toast('show');
    }
    else if (condition == 2) {
        $('.toast-head-text').html('Warning');
        $('.toast-body').html(res);
        $('.toast').addClass(' bg-warning text-dark');
        $('.toast').toast({ delay: 3000 }).toast('show');
    }
    else {
        $('.toast-body').html(res);
        $('.toast-head-text').html('Danger');
        $('.toast').addClass(' bg-danger text-white ');
        $('.toast').toast({ delay: 3000 }).toast('show');
    }
};

//header
function topheadcart() {
    var currency = localStorage.getItem("currency");
    var cart = "[]";
    var chkLScart = localStorage.getItem("_cartitems");
    if (chkLScart == null) {
        localStorage.setItem("_cartitems", cart);
    }

    var wishlist = "[]";
    var chkLSwishlist = localStorage.getItem("_Wishlistitems");
    if (chkLSwishlist == null) {
        localStorage.setItem("_Wishlistitems", wishlist);
    }


    var gift = "[]";
    var chkLSgift = localStorage.getItem("_giftitems");
    if (chkLSgift == null) {
        localStorage.setItem("_giftitems", gift);
    }

    var gifts = getgiftLS();
    var data = getCartLS();
    var html = '';
    var totalPrice = 0;
    var totalQty = data.length;


    html += '<div class="cart-height scrollbar" id="style2">'
    for (var i = 0; i < data.length; i++) {
        var giftPrice = 0;
        /* totalQty += Number(data[i].Qty);*/
        totalPrice += data[i].Qty * data[i].UPrice;
        html += '<li class="cart-item" >'
            + '<div class="cart-image">'
        if (data[i].Image == "" || data[i].Image == null) {
            html += '<a href="/Product/ProductDetails?ItemID=' + data[i].ItemID + '"><img alt="" src="/Content/assets/images/NA.png"></a>'
        }
        else {
            html += '<a href="/Product/ProductDetails?ItemID=' + data[i].ItemID + '"><img alt="" src="https://retail.premium-pos.com' + data[i].Image + '"></a>'
        }
        html += '</div>'
            + '<div class="cart-title">'
            + '<a href="/Product/ProductDetails?ItemID=' + data[i].ItemID + '">'
            + '<h4 class="mb-0 lh-16">' + data[i].Qty + ' x ' + data[i].Title + '</h4>'
            + '</a>'
        //+ '<button class="bg-transparent border-0 text-danger" onclick="removeCartItem(' + data[i].Key + '); return false;"><i class="h6 ion-trash-a mb-0"></i></button></div>'
        if (gifts.length > 0) {
            var _dataGiftFilter = gifts.filter(function (obj) {
                return (obj.ItemKey === data[i].Key);
            });

            for (var j = 0; j < _dataGiftFilter.length; j++) {
                totalPrice += _dataGiftFilter[j].DisplayPrice;
                giftPrice += _dataGiftFilter[j].DisplayPrice;
                html += '<p class="mb-0 text-default small lh-16 ">' + '-' + _dataGiftFilter[j].Title + '</p>'
            }
        }
        html += '<div class="price-box"><span class="new-price">' + currency + ' ' + ((data[i].Qty * data[i].UPrice) + giftPrice).toFixed(2) + '</span>'
            + '</div>'
            + '</li>'
    }

    html += '</div>'

        + '<li class="subtotal-titles">'
        + '<div class="subtotal-titles">'
        + ' <h3 data-translate="000co23">Sub-Total :</h3><span>' + currency + ' ' + totalPrice.toFixed(2) + '</span>'
        + ' </div>'
        + ' </li>'
        + ' <li class="mini-cart-btns">'
        + ' <div class="cart-btns">'
        + ' <a href="/order/cart"><span data-translate="000aa5">View cart</span></a>'
        + ' <a href="/order/checkout" ><span data-translate="000aa6">Checkout</span></a>'
        + ' </div>'
        + ' </li>'

    if (data.length > 0) {
        $(".head-cart").show();
    }
    else {
        $(".head-cart").hide();
    }
    $(".head-cart").html(html);
    $("#cart-total").html(totalQty);
};




//cart
function cartitem() {
    var currency = localStorage.getItem("currency");

    var gifts = getgiftLS();
    var total = 0;
    var data = getCartLS();
    var html = '';
    var totalPrice = 0;
    var totalQty = 0;


    for (var i = 0; i < data.length; i++) {
        var giftPrice = 0;
        totalQty += Number(data[i].Qty);
        totalPrice += data[i].Qty * data[i].UPrice;

        html += '<tr>'
        if (data[i].Image == "" || data[i].Image == null) {
            html += '<td class="plantmore-product-thumbnail"><a href="/Product/ProductDetails?ItemID=' + data[i].ItemID + '"><img class="cart-img" src="/Content/assets/images/NA.png" alt=""></a></td>'
        }
        else {
            html += '<td class="plantmore-product-thumbnail"><a href="/Product/ProductDetails?ItemID=' + data[i].ItemID + '"><img class="cart-img" src="https://retail.premium-pos.com/' + data[i].Image + '" alt=""></a></td>'
        }

        html += '<td class="plantmore-product-name">'
            + '<p><a href="/Product/ProductDetails?ItemID=' + data[i].ItemID + '">' + data[i].Title + '</a></p>'
        if (gifts.length > 0) {


            var _dataGiftFilter = gifts.filter(function (obj) {
                return (obj.ItemKey === data[i].Key);
            });

            for (var j = 0; j < _dataGiftFilter.length; j++) {
                //if (gifts[j].ItemKey = data[i].Key) {
                //gift
                html += '<p class="addon"> Addon Products</p>'
                html += '<div class="d-flex flex-wrap justify-content-center mb-3 gift-in-cart border">'
                    + '<div class="p-2 img"><img src="https://retail.premium-pos.com' + _dataGiftFilter[j].Image + '" alt=""></div>'
                    + '<div class="p-2 align-self-center"><p>' + _dataGiftFilter[j].Title + '</p></div>'
                    + '<div class="p-2 align-self-center "><p class="badge badge-dark"><span class="currency-text mx-0 text-white"></span>' + currency + ' ' + _dataGiftFilter[j].DisplayPrice + '</p></div>'
                    + '<div class="p-2 align-self-center"><button class="bg-transparent border-0 text-danger" onclick="removeCartGift(' + _dataGiftFilter[j].Key + '); return false;"><i class="h6 ion-trash-a mb-0"></i></button></div>'
                    + '</div>'
                //gift end
                //}
                totalPrice += _dataGiftFilter[j].DisplayPrice;
                giftPrice += _dataGiftFilter[j].DisplayPrice;
            }
        }
        html += '</td>'
            + '<td class="plantmore-product-price"><span class="amount"><span class="currency-text mx-0"></span>' + currency + ' ' + data[i].UPrice.toFixed(2) + '</span></td>'
            + '<td class="plantmore-product-quantity">'
            + '<input id="qty' + data[i].Key + '"  name="qty' + data[i].Key + '" onchange="changeQty(' + data[i].Key + ',' + data[i].UPrice + '); return false;" class="Quantity" value="' + data[i].Qty + '" type="number">'
            + '</td>'
            + '<td class="product-subtotal">' + currency + ' ' + '<span class="amount totalprice"  id="tprice' + data[i].Key + '">' + ((data[i].Qty * data[i].UPrice) + giftPrice).toFixed(2) + '</span></td>'
            + '<td class="plantmore-product-remove"><button class="bg-transparent border-0 text-danger" onclick="removeCartItem(' + data[i].Key + '); return false;"><i class="h3 ion-trash-a mb-0"></i></button></td>'
            + '</tr>'
    }


    if (data.length > 0) {
        $(".cart-items").html(html);
        $("#check-btn").show();
    }
    else {
        $("#cart-table").html("No Item added");
        $("#check-btn").hide();
    }
    $(".subtotal").html(currency + ' ' + totalPrice.toFixed(2));
    $(".totalamount").html(currency + ' ' + totalPrice.toFixed(2));


}

function changeQty(key, price) {

    if ($('#qty' + key).val() > 0) {

        var cartItems = getCartLS();
        var oldQty = 0;
        var newQty = parseInt($('#qty' + key).val());
        
        for (var i = 0; i < cartItems.length; i++) {
            if (cartItems[i].Key == key) {
                oldQty = parseInt(cartItems[i].Qty);
                cartItems[i].Qty = newQty;
                cartItems[i].Price = cartItems[i].Qty * price;
                $('#tprice' + key).html(cartItems[i].Price.toFixed(2));
                
                // Scale gifts proportionally
                if (oldQty > 0 && newQty > 0) {
                    var qtyRatio = newQty / oldQty;
                    var giftItems = getgiftLS();
                    
                    giftItems.forEach(function(gift) {
                        if (gift.ItemKey === key) {
                            var originalQty = parseInt(gift.OriginalQty) || 1;
                            gift.Qty = Math.round(originalQty * qtyRatio);
                        }
                    });
                    
                    setgiftLS(giftItems);
                }
            }
        }

        setCartLS(cartItems);
        cartitem();
        topheadcart();
    }
    else {
        $('#qty' + key).val(1);
    }


}
function removeCartItem(ele) {

    var chkLScart = getCartLS();
    var chkLSgift = getgiftLS();
    //var delRow = chkLScart.filter(obj => obj.Key === ele);

    chkLScart.forEach(function (item, index) {
        item.Key === ele && chkLScart.splice(index, 1);
    });

    var delRow = chkLSgift.filter(obj => obj.ItemKey === ele);
    chkLSgift.forEach(function (item, index) {
        item.ItemKey === ele && chkLSgift.splice(index, delRow.length);
    });
    setCartLS(chkLScart);
    setgiftLS(chkLSgift);

    cartitem();
    topheadcart();
}
function removeCartGift(ele) {

    var chkLSgift = getgiftLS();


    var delRow = chkLSgift.filter(obj => obj.Key === ele);
    chkLSgift.forEach(function (item, index) {
        item.Key === ele && chkLSgift.splice(index, delRow.length);
    });
    setgiftLS(chkLSgift);

    cartitem();
    topheadcart();

}
function addtocart(ItemID, Title, Image, Price, Qty, ProNote) {
    ProNote = ProNote == undefined ? "" : ProNote;
    var _Key = Math.floor((Math.random() * 1000) + 1);
    $('#hdnItemKey').val(_Key);

    var arrTemp = [];
    arrTemp = getCartLS();
    arrTemp.push({ ItemID: ItemID, Title: Title, Image: Image, UPrice: Price, Price: Price, Qty: Qty, ProNote: ProNote, Key: _Key });
    setCartLS(arrTemp);
    topheadcart();
}
function setCartLS(arr) {
    var getCartItem = localStorage.getItem("_cartitems");
    if (getCartItem != null) {

    }
    localStorage.setItem("_cartitems", JSON.stringify(arr));
}
function getCartLS() {
    var getCartItem = localStorage.getItem("_cartitems");
    if (getCartItem != null && getCartItem != "")
        return JSON.parse(getCartItem);
    else
        return JSON.parse("[]");
}



//Wishlist
function addtoWishlist(ItemID, Title, Image, Price, Instock, Qty) {

    var arrTemp = [];
    arrTemp = getWishlistLS();
    arrTemp.push({ ItemID: ItemID, Title: Title, Image: Image, Price: Price, Instock: Instock, Qty: Qty, Key: Math.floor((Math.random() * 1000) + 1) });
    setWishlistLS(arrTemp);
}
function setWishlistLS(arr) {
    var getWishlistItem = localStorage.getItem("_Wishlistitems");
    if (getWishlistItem != null) {
        localStorage.setItem("_Wishlistitems", JSON.stringify(arr));
    }
}
function getWishlistLS() {
    var getWishlistItem = localStorage.getItem("_Wishlistitems");
    if (getWishlistItem != null && getWishlistItem != "")
        return JSON.parse(getWishlistItem);
    else
        return JSON.parse("[]");
}
function removeWishlistitem(ele) {

    var chkLS = getWishlistLS();

    chkLS.splice(chkLS.filter(obj => obj.Key === ele), 1);
    setWishlistLS(chkLS);
    GetWishListItems();
}

function GetWishListItems() {
    var currency = localStorage.getItem("currency");
    var total = 0;
    var chkLSWishlist = localStorage.getItem("_Wishlistitems");
    var data = JSON.parse(chkLSWishlist);
    var html = '';
    var totalPrice = 0;
    var totalQty = 0;

    for (var i = 0; i < data.length; i++) {
        totalQty += Number(data[i].Qty);
        totalPrice += data[i].Price;
        html += '<tr>'
        if (data[i].Image == "" || data[i].Image == null) {
            html += '<td class="plantmore-product-thumbnail"><a href="/Product/ProductDetails?ItemID=' + data[i].ItemID + '"><img class="wishlist-img" src="/Content/assets/images/NA.png" alt=""></a></td>'
        }
        else {
            html += '<td class="plantmore-product-thumbnail"><a href="/Product/ProductDetails?ItemID=' + data[i].ItemID + '"><img class="wishlist-img" src="http://admin.karachiflora.com/' + data[i].Image + '" alt=""></a></td>'
        }

        html += '<td class="plantmore-product-name"><a href="#">' + data[i].Title + '</a></td>'
            + '<td class="plantmore-product-price"><span class="currency-text mx-0">' + currency + ' ' + data[i].Price.toFixed(2) + '</span></td>'
            + '<td class="plantmore-product-stock-status"><span class="stockcheck">' + data[i].Instock + '</span></td>'
            + '<td class="plantmore-product-add-cart"><a class="btn btn-default btn-small" href="/Product/ProductDetails?ItemID=' + data[i].ItemID + '">Add to Cart</a></td>'
            //+ "<td class='plantmore-product-add-cart'><a href='#' class='addItemLS ' onclick='addtocart("+data[i].ItemID+","+data[i].Title+"','"+ data[i].Image +"',"+ data[i].Price +",1);toast('Item Added to Cart', 1); return false;'>add to cart</a></td>"
            + '<td class="plantmore-product-remove"><button class="bg-transparent border-0 text-danger" onclick="removeWishlistitem(' + data[i].Key + '); return false;"><i class="h5 ion-trash-a mb-0"></i></a></td>'
            + '</tr>'

    }
    if (data.length > 0) {
        $(".wishlist-items").html(html);
    }
    else {
        $("#ytdTable").html("You donot have any item in favourites ");
    }


};

function StockActiveColor() {

    $("#ytdTable span.stockcheck").each(function () {

        var stock = $(this).html();

        if (stock == "True") {
            $(this).html("In Stock");
            $(this).css("color", "green");
        }
        else {
            $(this).html("In Stock");
            $(this).css("color", "black");
        }
    });
};

//Currency
var currency = "Rs.";
var currencyLS = localStorage.getItem("currency");
if (currencyLS == null) {
    localStorage.setItem("currency", currency);
}
else {
    localStorage.setItem("currency", "RS.");
}
function ShowText() {
    var currency = localStorage.getItem("currency");
    $(".currency-text").text(currency);
};


//form validation
(function () {
    'use strict';
    window.addEventListener('load', function () {
        // Get the forms we want to add validation styles to
        var forms = document.getElementsByClassName('needs-validation');
        // Loop over them and prevent submission
        var validation = Array.prototype.filter.call(forms, function (form) {

            form.addEventListener('submit', function (event) {
                if (form.checkValidity() === false) {
                    event.preventDefault();
                    event.stopPropagation();
                }
                form.classList.add('was-validated');
            }, false);
        });
    }, false);
})();


// Gift Functions - Add Cart Item with Gifts
function AddCartWithGifts(itemId, itemTitle, qty) {
    // Get selected gifts from modal
    var selectedGifts = [];
    
    $('#giftModal .gift-checkbox:checked').each(function() {
        var giftData = {
            id: $(this).data('id'),
            title: $(this).data('title'),
            image: $(this).data('image-src'),
            price: parseFloat($(this).data('price'))
        };
        selectedGifts.push(giftData);
    });
    
    // Get current cart item data from ProductDetails page
    var productImage = $('img[src*="product"]').attr('src') || '/Content/assets/images/NA.png';
    
    // Extract image from button data attributes or page
    var itemImage = $('.buy-now.add-to-cart').data('image-src') || productImage;
    var itemPrice = parseFloat($('.buy-now.add-to-cart').data('price')) || 0;
    
    // Generate unique key for this cart item
    var itemKey = Math.floor((Math.random() * 10000) + 1);
    
    // Add item to cart
    var cartItems = getCartLS();
    cartItems.push({
        ItemID: itemId,
        Title: itemTitle,
        Image: itemImage,
        UPrice: itemPrice,
        Price: itemPrice * qty,
        Qty: qty,
        ProNote: '',
        Key: itemKey
    });
    setCartLS(cartItems);
    
    // Add gifts to gift items list
    if (selectedGifts.length > 0) {
        var giftItems = getgiftLS();
        
        selectedGifts.forEach(function(gift) {
            giftItems.push({
                ItemID: gift.id,
                GiftID: gift.id,
                Title: gift.title,
                Image: gift.image,
                DisplayPrice: gift.price,
                DiscountedPrice: 0,
                Qty: qty,  // Gift quantity matches main item quantity
                OriginalQty: qty,  // Store original qty for proportional scaling
                Key: Math.floor((Math.random() * 10000) + 1),
                ItemKey: itemKey  // Link gift to main cart item
            });
        });
        
        setgiftLS(giftItems);
    }
    
    // Close modal and update cart display
    $('#giftModal').modal('hide');
    topheadcart();
    cartitem();
    
    // Show success message
    toast('Item added to cart with gifts!', 1);
}

//Index - Home

function getmail() {
    var email = $(".SubscribeEmail").val();
    $('.SubscribeEmail').val("");

    $.ajax({
        type: "Get",
        url: '/Home/Subscribe?email=' + email,
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        success: function (res) {


        },
        error: function (xhr, textStatus, errorThrown) {
            //
        }
    });
};
