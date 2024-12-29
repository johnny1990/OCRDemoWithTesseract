const video = document.getElementById('camera');
const canvas = document.getElementById('snapshot');
const captureButton = document.getElementById('capture');
const ocrResult = document.getElementById('ocrResult');
const suggestions = document.getElementById('suggestions');

navigator.mediaDevices.getUserMedia({ video: true })
    .then(stream => video.srcObject = stream)
    .catch(err => console.error("Webcam error:", err));

captureButton.addEventListener('click', () => {
    const context = canvas.getContext('2d');
    canvas.width = video.videoWidth;
    canvas.height = video.videoHeight;
    context.drawImage(video, 0, 0, canvas.width, canvas.height);

    canvas.toBlob(blob => {
        const formData = new FormData();
        formData.append('ImageFile', blob, 'snapshot.jpg');

        fetch('/Home/ProcessImage', {
            method: 'POST',
            body: formData
        })
            .then(response => response.json())
            .then(data => {
                ocrResult.textContent = data.ocrResult || 'No text found in image.';
                suggestions.innerHTML = '';
                (data.suggestions || []).forEach(suggestion => {
                    const li = document.createElement('li');
                    li.textContent = suggestion;
                    suggestions.appendChild(li);
                });
            })
            .catch(err => console.error("Error:", err));
    });
});