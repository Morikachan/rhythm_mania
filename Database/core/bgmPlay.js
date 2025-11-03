const soundBtn = document.querySelector("#soundButton");
const audio = document.querySelector("#bgm-play");

window.addEventListener(
  "click",
  function () {
    audio.play();
  },
  { once: true }
);

soundBtn.addEventListener("click", () => {
  if (soundBtn.classList.contains("active")) {
    soundBtn.innerHTML = "<i class='fa-solid fa-volume-xmark'></i>";
    audio.pause();
    audio.currentTime = 0;
  } else {
    audio.play();
    soundBtn.innerHTML = "<i class='fa-solid fa-volume-high'></i>";
  }
  soundBtn.classList.toggle("active");
});
