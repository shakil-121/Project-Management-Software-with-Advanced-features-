// SignalR Connection Setup
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/notificationHub")
    .configureLogging(signalR.LogLevel.Information)
    .build();

// Connection Events
connection.start().then(function () {
    console.log("✅ Notification Hub Connected");
}).catch(function (err) {
    console.error("❌ Notification Hub Connection Failed: ", err.toString());
});

// Receive Notification
connection.on("ReceiveNotification", function (notification) {
    console.log("🔔 New Notification Received:", notification);

    // Show browser notification
    showBrowserNotification(notification.message);

    // Update UI
    updateNotificationUI(notification);

    // Play sound
    playNotificationSound();
});

// Browser Notification
function showBrowserNotification(message) {
    if ("Notification" in window && Notification.permission === "granted") {
        new Notification("New Notification", { body: message, icon: "/icons/notification.png" });
    }
}

// Request Notification Permission
function requestNotificationPermission() {
    if ("Notification" in window && Notification.permission === "default") {
        Notification.requestPermission().then(permission => {
            console.log("Notification permission:", permission);
        });
    }
}

// Update Notification UI
function updateNotificationUI(notification) {
    // Update notification count
    const countElement = document.getElementById('notificationCount');
    if (countElement) {
        const currentCount = parseInt(countElement.textContent) || 0;
        countElement.textContent = currentCount + 1;
        countElement.style.display = 'block';
    }

    // Add to notification list
    addToNotificationList(notification);
}

// Add to notification dropdown
function addToNotificationList(notification) {
    const notificationList = document.getElementById('notificationList');
    if (notificationList) {
        const notificationItem = `
            <div class="notification-item ${notification.isRead ? '' : 'unread'}" data-id="${notification.id}">
                <div class="notification-message">${notification.message}</div>
                <div class="notification-time">${formatTime(notification.createdAt)}</div>
            </div>
        `;
        notificationList.insertAdjacentHTML('afterbegin', notificationItem);
    }
}

// Format time
function formatTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleTimeString('en-US', {
        hour: '2-digit',
        minute: '2-digit',
        hour12: true
    });
}

// Play notification sound
function playNotificationSound() {
    const audio = new Audio('/sounds/notification.mp3');
    audio.play().catch(e => console.log('Audio play failed:', e));
}

// Load notifications
async function loadNotifications() {
    try {
        const response = await fetch('/Chat/GetNotifications');
        const notifications = await response.json();

        const notificationList = document.getElementById('notificationList');
        if (notificationList) {
            notificationList.innerHTML = notifications.map(notification => `
                <div class="notification-item ${notification.isRead ? '' : 'unread'}" data-id="${notification.id}">
                    <div class="notification-message">${notification.message}</div>
                    <div class="notification-time">${formatTime(notification.createdAt)}</div>
                </div>
            `).join('');
        }
    } catch (error) {
        console.error('Error loading notifications:', error);
    }
}

// Mark as read
async function markAsRead(notificationId) {
    try {
        await fetch('/Chat/MarkNotificationAsRead', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify({ notificationId })
        });
    } catch (error) {
        console.error('Error marking as read:', error);
    }
}

// Update notification count
async function updateNotificationCount() {
    try {
        const response = await fetch('/Chat/GetUnreadNotificationCount');
        const data = await response.json();

        const countElement = document.getElementById('notificationCount');
        if (countElement) {
            countElement.textContent = data.count;
            countElement.style.display = data.count > 0 ? 'block' : 'none';
        }
    } catch (error) {
        console.error('Error updating notification count:', error);
    }
}

// Initialize
document.addEventListener('DOMContentLoaded', function () {
    requestNotificationPermission();
    updateNotificationCount();
    loadNotifications();
});