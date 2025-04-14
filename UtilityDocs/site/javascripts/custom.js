document.addEventListener("DOMContentLoaded", function() {
    // Create the dark mode toggle button
    const darkModeToggle = document.createElement("button");
    darkModeToggle.id = "dark-mode-toggle";
    darkModeToggle.textContent = "Toggle Dark Mode";
    darkModeToggle.style.position = "fixed";
    darkModeToggle.style.top = "10px";
    darkModeToggle.style.right = "10px";
    darkModeToggle.style.padding = "10px";
    darkModeToggle.style.backgroundColor = "#333";
    darkModeToggle.style.color = "#fff";
    darkModeToggle.style.border = "none";
    darkModeToggle.style.cursor = "pointer";
    
    // Append the button to the body (or any other place in the DOM)
    document.body.appendChild(darkModeToggle);
  
    // Check for previously saved user preference
    const isDarkMode = localStorage.getItem('darkMode') === 'true';
    if (isDarkMode) {
        document.body.classList.add('dark-mode');
    }

    // Toggle dark mode on button click
    darkModeToggle.addEventListener('click', function() {
        const darkModeElements = document.querySelectorAll('.wy-nav-content','.wy-grid-for-nav');
        darkModeElements.forEach(function(element) {
            element.classList.toggle('dark-mode');
        });

        // Save user preference to localStorage
        const darkModeEnabled = document.body.classList.contains('dark-mode');
        localStorage.setItem('darkMode', darkModeEnabled);
    });
});