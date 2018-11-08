function addDeleteButtons () {
  var $deleteButtons = Array.prototype.slice.call(document.querySelectorAll('.is-delete'), 0);
  if ($deleteButtons.length > 0) {
    $deleteButtons.forEach(function($el) {
      $el.addEventListener('click', function () {
        var target = $el.dataset.href;
        var xhr = new XMLHttpRequest();
        xhr.open("DELETE", target, true);
        xhr.setRequestHeader("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8");
        xhr.onload = function () {
          Turbolinks.visit(window.location.href, { action: 'replace' });
        }
        xhr.send(null);
      });
    });
  }
}

function setLastDoneAsNow() {
  document.getElementsByName('last_done_at')[0].value = new Date().toISOString();
}

function clearTurbolinksCacheOnLogoutClick() {
  var anchors = document.getElementsByClassName('logout-link');
  for (var i = 0; i < anchors.length; i++) {
    var anchor = anchors[i];
    anchor.addEventListener('click', function () {
      Turbolinks.clearCache();
    });
  }
}

function addOnclickToSetLastDoneAsNowButtonIfPresent() {
  var button = document.getElementById('set_last_done_now_button');
  if (button !== null) {
    button.onclick = setLastDoneAsNow;
  }
}

function setUpThePage() {
  addDeleteButtons();
  addOnclickToSetLastDoneAsNowButtonIfPresent();
  clearTurbolinksCacheOnLogoutClick();
}

document.addEventListener('DOMContentLoaded', setUpThePage);
document.addEventListener('turbolinks:load', setUpThePage);
