"use strict";

// ============================================
// GLOBAL STATE & CONFIGURATION
// ============================================
const connection = new signalR.HubConnectionBuilder().withUrl("https://localhost:7092/chatHub").build();
const userData = JSON.parse(sessionStorage.getItem("user"));
let conversationData = []; // Cache conversation data từ API

// ============================================
// UTILITY FUNCTIONS
// ============================================
function normalizeDisplayName(value) {
    return (value || "").trim().toLowerCase();
}

function getUserInfoById(userId) {
    if (!userId) return null;
    const overlayUser = document.querySelector(`.overlay-user[data-id="${userId}"]`);
    if (!overlayUser) return null;

    const nameAttr = overlayUser.getAttribute("data-username");
    const avatarAttr = overlayUser.getAttribute("data-avatar");

    let name = nameAttr || "";
    if (!name) {
        const nameEl = overlayUser.querySelector(".overlay-user-name");
        if (nameEl) {
            name = nameEl.textContent || "";
        }
    }

    let avatar = avatarAttr || "";
    if (!avatar) {
        const avatarImg = overlayUser.querySelector(".overlay-avatar-img");
        if (avatarImg && avatarImg.src) {
            avatar = avatarImg.src;
        }
    }

    return { name, avatar };
}

function resolveConversationAvatar(displayName, conversationId) {
    const headerAvatar = document.getElementById("chat-header-avatar");
    if (headerAvatar && headerAvatar.src) {
        return headerAvatar.src;
    }
    const entry = findConversationEntry(displayName, conversationId);
    if (entry) {
        const dataAvatar = entry.getAttribute("data-avatar");
        if (dataAvatar) {
            return dataAvatar;
        }
        const img = entry.querySelector(".conv-avatar");
        if (img && img.src) {
            return img.src;
        }
    }
    return `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName || "User")}&background=5b6bff&color=fff`;
}

// ============================================
// CONVERSATION LIST MANAGER
// ============================================
const ConversationManager = {
    findEntry(displayName, conversationId) {
        const convList = document.getElementById("conversation-list-js");
        if (!convList) return null;

        // Ưu tiên tìm theo conversationId nếu có
        if (conversationId) {
            const entryById = convList.querySelector(`.conv-entry[data-convid="${conversationId}"]`);
            if (entryById) return entryById;
        }

        const normalizedName = normalizeDisplayName(displayName);
        const convEntries = convList.querySelectorAll(".conv-entry");
        for (let i = 0; i < convEntries.length; i++) {
            const entry = convEntries[i];
            const nameEl = entry.querySelector(".conv-name");
            if (nameEl && normalizeDisplayName(nameEl.textContent) === normalizedName) {
                return entry;
            }
        }
        return null;
    },

    // Loại bỏ empty state message "Không có cuộc trò chuyện nào."
    removeEmptyState() {
        const convList = document.getElementById("conversation-list-js");
        if (!convList) return;

        // Tìm tất cả các entry không có data-convid và không có class conv-name (empty state)
        const entries = convList.querySelectorAll(".conv-entry");
        entries.forEach(entry => {
            const hasConvId = entry.getAttribute("data-convid");
            const hasConvName = entry.querySelector(".conv-name");
            const textContent = entry.textContent.trim();
            
            // Nếu entry không có data-convid, không có conv-name, và chứa text "Không có cuộc trò chuyện nào."
            if (!hasConvId && !hasConvName && textContent.includes("Không có cuộc trò chuyện nào")) {
                entry.remove();
            }
        });
    },

    upsertEntry(displayName, messagePreview, conversationId, avatarUrl) {
        const convList = document.getElementById("conversation-list-js");
        if (!convList) return;

        const existingEntry = this.findEntry(displayName, conversationId);
        const now = new Date();
        const timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

        if (existingEntry) {
            // Update existing entry
            const lastMsgEl = existingEntry.querySelector(".conv-lastmsg");
            if (lastMsgEl) {
                lastMsgEl.textContent = messagePreview;
            }

            const timeEl = existingEntry.querySelector(".conv-time");
            if (timeEl) {
                timeEl.textContent = timeStr;
            }

            if (conversationId) {
                existingEntry.setAttribute("data-convid", conversationId);
            }
            if (avatarUrl) {
                existingEntry.setAttribute("data-avatar", avatarUrl);
                const avatarImg = existingEntry.querySelector(".conv-avatar");
                if (avatarImg) {
                    avatarImg.src = avatarUrl;
                }
            }

            // Move to top
            if (convList.firstChild !== existingEntry) {
                convList.insertBefore(existingEntry, convList.firstChild);
            }
            return;
        }

        // Loại bỏ empty state trước khi thêm conversation mới
        this.removeEmptyState();

        // Create new entry
        const convHtml = `
        <div class="conv-entry" data-convid="${conversationId || ""}" data-name="${displayName}" data-avatar="${avatarUrl}">
            <img class="conv-avatar" src="${avatarUrl}" alt="avatar" />
            <div class="conv-info">
                <div class="conv-name">${displayName}</div>
                <div class="conv-lastmsg">${messagePreview}</div>
            </div>
            <div class="conv-time">${timeStr}</div>
        </div>`;

        convList.insertAdjacentHTML("afterbegin", convHtml);
    },

    setActive(conversationId) {
        document.querySelectorAll('.conv-entry').forEach(e => e.classList.remove('active'));
        if (conversationId) {
            const targetEntry = document.querySelector(`.conv-entry[data-convid="${conversationId}"]`);
            if (targetEntry) {
                targetEntry.classList.add('active');
            }
        }
    }
};

// Alias for backward compatibility
const findConversationEntry = ConversationManager.findEntry.bind(ConversationManager);

// ============================================
// MESSAGE RENDERER
// ============================================
const MessageRenderer = {
    // Render một tin nhắn với xử lý consecutive messages
    renderMessage(messageText, isMine, avatar, isConsecutive = false) {
        if (isConsecutive) {
            return `<div class="msg-row ${isMine ? 'mine' : 'other'} consecutive">
                        <div class="chat-message">${this.escapeHtml(messageText)}</div>
                    </div>`;
        } else {
            return `<div class="msg-row ${isMine ? 'mine' : 'other'}">
                        ${!isMine ? `<img class="avatar-small" src="${this.escapeHtml(avatar)}" alt="Ava" />` : ''}
                        <div class="chat-message">${this.escapeHtml(messageText)}</div>
                    </div>`;
        }
    },

    // Render toàn bộ messages của một conversation (khi load conversation)
    renderAllMessages(messages, conversationAvatar) {
        if (!messages || messages.length === 0) return '';

        let html = '';
        for (let i = 0; i < messages.length; i++) {
            const msg = messages[i];
            const isMine = msg.isMine;
            const prevMsg = i > 0 ? messages[i - 1] : null;
            const isConsecutive = prevMsg && prevMsg.isMine === isMine;

            html += this.renderMessage(msg.text, isMine, conversationAvatar, isConsecutive);
        }
        return html;
    },

    // Tìm tin nhắn cuối cùng để check consecutive
    getLastMessageRow() {
        const messagesDiv = document.getElementById("chat-messages-js");
        if (!messagesDiv) return null;

        const allChildren = Array.from(messagesDiv.children);
        for (let i = allChildren.length - 1; i >= 0; i--) {
            const child = allChildren[i];
            if (child.classList.contains('msg-row') && !child.classList.contains('intro-banner-user')) {
                return child;
            }
        }
        return null;
    },

    // Check xem tin nhắn mới có consecutive với tin nhắn trước không
    checkConsecutive(isMine) {
        const lastMessageRow = this.getLastMessageRow();
        if (!lastMessageRow) return false;

        const lastMine = lastMessageRow.classList.contains('mine');
        return lastMine === isMine;
    },

    // Escape HTML để tránh XSS
    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    },

    // Scroll to bottom với smooth animation
    scrollToBottom() {
        const messagesDiv = document.getElementById("chat-messages-js");
        if (!messagesDiv) return;

        setTimeout(() => {
            messagesDiv.scrollTo({
                top: messagesDiv.scrollHeight,
                behavior: 'smooth'
            });
        }, 10);
    }
};

// ============================================
// UI CONTROLLER
// ============================================
const UIController = {
    // Render chat panel header
    updateHeader(conversationId, userId, displayName, avatar) {
        const headerAvatar = document.getElementById("chat-header-avatar");
        const headerName = document.getElementById("chat-header-name");
        const chatHeader = document.getElementById("chat-header-js");

        if (!chatHeader) return;

        // Create header if doesn't exist
        if (!headerName) {
            chatHeader.innerHTML = `
                <img class="avatar" src="${this.escapeHtml(avatar)}" alt="Receiver" id="chat-header-avatar" />
                <span class="chat-user" data-convid="${conversationId || ""}" userid="${userId || ""}" id="chat-header-name">${this.escapeHtml(displayName)}</span>
            `;
        } else {
            if (headerAvatar) headerAvatar.src = avatar;
            if (headerName) {
                headerName.textContent = displayName;
                headerName.setAttribute("data-convid", conversationId || "");
                headerName.setAttribute("userid", userId || "");
            }
        }
    },

    // Render intro banner
    renderBanner(displayName, avatar, hasMessages = true) {
        const messagesDiv = document.getElementById("chat-messages-js");
        if (!messagesDiv) return;

        const desc = hasMessages
            ? `Bạn đang trò chuyện với <b>${this.escapeHtml(displayName)}</b>. Hãy giữ lịch sự & tích cực!`
            : `Chưa có cuộc trò chuyện nào. Bắt đầu nhắn tin với ${this.escapeHtml(displayName)} nhé!`;

        const bannerHtml = `
            <div class="intro-banner-user" id="chat-banner-intro-js">
                <img class="intro-avatar" src="${this.escapeHtml(avatar)}" alt="Chat user" id="chat-banner-avatar" />
                <div class="intro-name" id="chat-banner-name">${this.escapeHtml(displayName)}</div>
                <div class="intro-desc">${desc}</div>
            </div>`;

        return bannerHtml;
    },

    // Bootstrap chat panel cho receiver (lần đầu nhận tin nhắn)
    bootstrapReceiverPanel(conversationId, senderId, displayName, avatarUrl) {
        this.updateHeader(conversationId, senderId, displayName, avatarUrl);

        const messagesDiv = document.getElementById("chat-messages-js");
        if (messagesDiv) {
            messagesDiv.innerHTML = this.renderBanner(displayName, avatarUrl, true);
        }

        const chatInputContainer = document.querySelector(".chat-input-container");
        if (chatInputContainer) {
            chatInputContainer.style.visibility = "visible";
        }
    },

    // Show/hide empty state
    toggleEmptyState(show) {
        const emptyChatState = document.querySelector('.empty-chat-state');
        if (emptyChatState) {
            emptyChatState.style.display = show ? 'flex' : 'none';
        }

        const chatInputContainer = document.querySelector(".chat-input-container");
        if (chatInputContainer) {
            chatInputContainer.style.visibility = show ? 'hidden' : 'visible';
        }
    },

    // Clear messages area (giữ lại banner)
    clearMessages() {
        const messagesDiv = document.getElementById("chat-messages-js");
        if (!messagesDiv) return;

        const banner = messagesDiv.querySelector('.intro-banner-user');
        messagesDiv.innerHTML = '';
        if (banner) {
            messagesDiv.appendChild(banner);
        }
    },

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
};

// ============================================
// CONVERSATION RENDERER
// ============================================
const ConversationRenderer = {
    // Render toàn bộ conversation (header + banner + messages)
    renderConversation(conv) {
        if (!conv) {
            console.error('Conversation not found');
            return;
        }

        // Xác định userId của người đang chat với (không phải current user)
        const currentUserId = userData.id;
        const otherUserId = (conv.senderId === currentUserId) ? conv.receiverId : conv.senderId;

        // Update header
        UIController.updateHeader(conv.id, otherUserId, conv.name, conv.avatar);

        // Update active state
        ConversationManager.setActive(conv.id);

        // Render messages area
        const messagesDiv = document.getElementById('chat-messages-js');
        if (!messagesDiv) {
            console.error('Messages div not found');
            return;
        }

        // Clear và render lại
        messagesDiv.innerHTML = this.renderBanner(conv.name, conv.avatar, conv.messages && conv.messages.length > 0);

        // Render messages
        if (conv.messages && conv.messages.length > 0) {
            const messagesHtml = MessageRenderer.renderAllMessages(conv.messages, conv.avatar);
            messagesDiv.insertAdjacentHTML('beforeend', messagesHtml);
        }

        // Hide empty state
        UIController.toggleEmptyState(false);

        // Scroll to bottom
        MessageRenderer.scrollToBottom();
    },

    // Render empty conversation (chưa có tin nhắn)
    renderEmptyConversation(userId, displayName, avatar) {
        const safeName = displayName || 'Người dùng';
        const safeAvatar = avatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(safeName)}&background=5b6bff&color=fff`;

        // Update header
        UIController.updateHeader(null, userId, safeName, safeAvatar);

        // Remove active state
        ConversationManager.setActive(null);

        // Render empty state
        const messagesDiv = document.getElementById('chat-messages-js');
        if (!messagesDiv) return;

        messagesDiv.innerHTML = UIController.renderBanner(safeName, safeAvatar, false);

        // Hide empty state
        UIController.toggleEmptyState(false);
    },

    renderBanner(displayName, avatar, hasMessages) {
        return UIController.renderBanner(displayName, avatar, hasMessages);
    }
};

// ============================================
// CONVERSATION LOADER
// ============================================
const ConversationLoader = {
    // Load conversations từ API
    async loadConversations() {
        try {
            const response = await fetch('/api/message');
            if (response.status === 200) {
                conversationData = await response.json() || [];
                console.log('Conversations loaded:', conversationData);
                return conversationData;
            } else {
                console.warn("No Conversation!");
                conversationData = [];
                return [];
            }
        } catch (error) {
            console.error("Error loading conversations:", error);
            conversationData = [];
            return [];
        }
    },

    // Select conversation by ID
    selectConversationById(id) {
        if (!id || isNaN(id)) {
            console.error('Invalid conversation ID');
            return;
        }

        const conv = conversationData.find(x => x.id === parseInt(id));
        if (!conv) {
            console.warn('Conversation not found for ID:', id);
            return;
        }

        ConversationRenderer.renderConversation(conv);
    }
};

// ============================================
// CONVERSATION DATA MANAGER
// ============================================
const ConversationDataManager = {
    // Cập nhật conversationData khi có tin nhắn mới
    updateConversationData(conversationId, senderId, receiverId, displayName, message, avatar, isMine, isFirstChat) {
        const currentUserId = userData.id;
        const otherUserId = isMine ? receiverId : senderId;

        let conv = null;

        // Tìm conversation theo ID nếu có
        if (conversationId && conversationId !== "" && conversationId !== "null") {
            conv = conversationData.find(c => c.id === parseInt(conversationId));
        }

        // Nếu không tìm thấy theo ID, tìm theo user ID (cho trường hợp chưa có conversationId)
        if (!conv) {
            conv = conversationData.find(c => {
                const matchesUser = (c.senderId === otherUserId && c.receiverId === currentUserId) ||
                    (c.receiverId === otherUserId && c.senderId === currentUserId);
                const hasNoId = !c.id || c.id === null;
                return matchesUser && hasNoId;
            });
        }

        // Nếu vẫn không tìm thấy, tạo conversation mới
        if (!conv) {
            conv = {
                id: conversationId && conversationId !== "" && conversationId !== "null" ? parseInt(conversationId) : null,
                name: displayName,
                senderId: isMine ? currentUserId : senderId,
                receiverId: isMine ? receiverId : currentUserId,
                avatar: avatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName || "User")}&background=5b6bff&color=fff`,
                lastMessage: message,
                lastMessageTime: new Date().toISOString(),
                messages: []
            };

            // Thêm vào đầu mảng
            conversationData.unshift(conv);
        } else {
            // Cập nhật thông tin conversation nếu cần
            if (!conv.name || conv.name === "") {
                conv.name = displayName;
            }
            if (!conv.avatar || conv.avatar === "") {
                conv.avatar = avatar || `https://ui-avatars.com/api/?name=${encodeURIComponent(displayName || "User")}&background=5b6bff&color=fff`;
            }
        }

        // Cập nhật LastMessage và LastMessageTime
        conv.lastMessage = message;
        conv.lastMessageTime = new Date().toISOString();

        // Thêm message mới vào Messages array
        const newMessage = {
            text: message,
            isMine: isMine
        };

        if (!conv.messages) {
            conv.messages = [];
        }

        conv.messages.push(newMessage);

        // Nếu conversation chưa có ID nhưng server đã trả về ID, cập nhật
        if ((!conv.id || conv.id === null) && conversationId && conversationId !== "" && conversationId !== "null") {
            conv.id = parseInt(conversationId);
        }

        // Di chuyển conversation lên đầu danh sách (most recent)
        const index = conversationData.indexOf(conv);
        if (index > 0) {
            conversationData.splice(index, 1);
            conversationData.unshift(conv);
        }

        return conv;
    },

    // Tìm conversation theo ID
    findConversationById(id) {
        if (!id) return null;
        return conversationData.find(c => c.id === parseInt(id));
    },

    // Tìm conversation theo user ID (cho trường hợp chưa có conversationId)
    findConversationByUserId(userId) {
        if (!userId) return null;
        return conversationData.find(c =>
            (c.senderId === userId || c.receiverId === userId) &&
            (!c.id || c.id === null)
        );
    },

    // Cập nhật conversation ID sau khi server trả về
    updateConversationId(oldId, newId) {
        if (!newId) return;

        // Tìm conversation theo oldId hoặc tìm conversation chưa có ID
        let conv = null;
        if (oldId) {
            conv = conversationData.find(c => c.id === parseInt(oldId));
        }

        // Nếu không tìm thấy, tìm conversation chưa có ID
        if (!conv) {
            conv = conversationData.find(c => !c.id || c.id === null);
        }

        if (conv) {
            conv.id = parseInt(newId);
        }
    }
};

// ============================================
// SIGNALR MESSAGE HANDLER
// ============================================
const SignalRHandler = {
    // Xử lý tin nhắn nhận được từ SignalR
    handleReceiveMessage(id, senderId, receiverId, user, message, isFirstChat) {
        const isSender = userData.id === senderId;
        const isReceiver = userData.id === receiverId;

        // Bỏ qua nếu không phải sender hoặc receiver
        if (!isSender && !isReceiver) {
            return;
        }

        // Xác định thông tin người còn lại
        let otherUserName = user || "";
        let explicitAvatar = null;

        if (isReceiver) {
            // Lấy thông tin từ danh bạ nếu có
            const senderInfo = getUserInfoById(senderId);
            if (senderInfo) {
                otherUserName = senderInfo.name || otherUserName;
                explicitAvatar = senderInfo.avatar || explicitAvatar;
            }
        }

        const displayName = otherUserName;
        const isMine = isSender;
        const otherUserId = isSender ? receiverId : senderId;

        // Xử lý avatar
        let conversationAvatar = explicitAvatar || resolveConversationAvatar(displayName, id);
        const chatHeaderAvatarEl = document.getElementById("chat-header-avatar");
        if (chatHeaderAvatarEl && chatHeaderAvatarEl.src) {
            conversationAvatar = chatHeaderAvatarEl.src;
        }

        // Kiểm tra xem có header hiện tại không
        let chatHeaderNameEl = document.getElementById("chat-header-name");
        const hadNoHeaderBefore = !chatHeaderNameEl;

        // Bootstrap UI cho receiver lần đầu (chỉ khi chưa có header)
        if (isReceiver && hadNoHeaderBefore) {
            // Chỉ bootstrap nếu đây là conversation đầu tiên từ người này
            // và chưa có header nào cả
            UIController.bootstrapReceiverPanel(id, senderId, displayName, conversationAvatar);
            chatHeaderNameEl = document.getElementById("chat-header-name");
        }

        // Kiểm tra xem có phải conversation đang active không
        // Sau khi bootstrap (nếu có), check lại
        const isActiveConversation = this.shouldRenderToActiveChat(id, otherUserId, isFirstChat, hadNoHeaderBefore && isReceiver);

        // CHỈ update header attributes nếu đang trong đúng conversation
        if (chatHeaderNameEl && isActiveConversation) {
            // Update header chỉ khi đúng conversation đang active
            chatHeaderNameEl.setAttribute("data-convid", id || "");
            chatHeaderNameEl.setAttribute("userid", otherUserId);

            // Update avatar nếu cần
            if (chatHeaderAvatarEl && conversationAvatar) {
                chatHeaderAvatarEl.src = conversationAvatar;
            }
        }

        // CHỈ render vào right panel nếu đang mở đúng conversation
        if (isActiveConversation) {
            this.appendMessageToChat(message, isMine, conversationAvatar);
            // Chỉ clear input khi là sender (tin nhắn của mình)
            if (isSender) {
                const chatInput = document.getElementById("chat-input-js");
                if (chatInput) {
                    chatInput.value = "";
                }
            }
        }

        // CẬP NHẬT conversationData ngay lập tức khi có tin nhắn mới
        // Điều này đảm bảo conversationData luôn được sync với tin nhắn real-time
        const updatedConv = ConversationDataManager.updateConversationData(
            id,
            senderId,
            receiverId,
            displayName,
            message,
            conversationAvatar,
            isMine,
            isFirstChat
        );

        // Nếu conversation có ID mới từ server, cập nhật vào conversationData
        if (id && id !== "" && id !== "null" && updatedConv && (!updatedConv.id || updatedConv.id === null)) {
            updatedConv.id = parseInt(id);
        }

        // LUÔN update conversation list (để hiển thị trong danh sách bên trái)
        this.updateConversationList(id, displayName, message, conversationAvatar, isSender);
    },

    // Kiểm tra xem có nên render vào active chat không
    // Cần check cả conversationId và userid để đảm bảo chính xác
    // Logic này đảm bảo chỉ render tin nhắn vào conversation đang được mở
    shouldRenderToActiveChat(conversationId, otherUserId, isFirstChat, isReceiverFirstTime = false) {
        const chatHeaderNameEl = document.getElementById("chat-header-name");

        // Trường hợp đặc biệt: receiver lần đầu nhận tin nhắn
        // Sau khi bootstrap UI, header đã được tạo và cần render tin nhắn đầu tiên vào
        if (isReceiverFirstTime && chatHeaderNameEl) {
            // Kiểm tra xem header vừa được tạo có khớp với tin nhắn này không
            // (header.userId = senderId, otherUserId = senderId khi là receiver)
            const headerUserId = chatHeaderNameEl.getAttribute("userid");
            return String(otherUserId) === String(headerUserId);
        }

        // Nếu chưa có header, không render (trừ trường hợp trên đã xử lý)
        if (!chatHeaderNameEl) {
            return false;
        }

        const activeConversationId = chatHeaderNameEl.getAttribute("data-convid");
        const activeUserId = chatHeaderNameEl.getAttribute("userid");

        // Case 1: Cả tin nhắn và active conversation đều có conversationId
        // Phải khớp CẢ conversationId VÀ userid để tránh render nhầm
        // Kiểm tra conversationId hợp lệ (không null, không rỗng, không phải "null")
        const hasValidConversationId = conversationId && conversationId !== "" && conversationId !== "null";
        const hasValidActiveConversationId = activeConversationId && activeConversationId !== "" && activeConversationId !== "null";
        
        if (hasValidConversationId && hasValidActiveConversationId) {
            // So sánh conversationId (convert sang string để đảm bảo so sánh đúng)
            const convIdMatch = String(conversationId) === String(activeConversationId);
            // So sánh userId (convert sang string để đảm bảo so sánh đúng)
            const userIdMatch = String(otherUserId) === String(activeUserId);
            return convIdMatch && userIdMatch;
        }

        // Case 2: Đây là conversation đầu tiên (chưa có conversationId)
        // Chỉ check userid, nhưng active conversation cũng phải chưa có conversationId
        if (isFirstChat || !conversationId) {
            // Active conversation chưa có ID => đang ở trạng thái mới
            if (!activeConversationId || activeConversationId === "" || activeConversationId === "null") {
                // Chỉ match nếu userid khớp
                return String(otherUserId) === String(activeUserId);
            }
            // Active conversation đã có ID nhưng tin nhắn này chưa có => không match
            return false;
        }

        // Case 3: Tin nhắn có conversationId nhưng active conversation chưa có
        // Hoặc các trường hợp khác không khớp
        return false;
    },

    // Append message vào chat panel
    appendMessageToChat(message, isMine, avatar) {
        const messagesDiv = document.getElementById("chat-messages-js");
        if (!messagesDiv) return;

        const isConsecutive = MessageRenderer.checkConsecutive(isMine);
        const messageHtml = MessageRenderer.renderMessage(message, isMine, avatar, isConsecutive);

        messagesDiv.insertAdjacentHTML('beforeend', messageHtml);
        MessageRenderer.scrollToBottom();
    },

    // Update conversation list entry
    updateConversationList(conversationId, displayName, message, avatar, isSender) {
        try {
            const preview = isSender ? `Bạn: ${message}` : message;
            ConversationManager.upsertEntry(displayName, preview, conversationId, avatar);
        } catch (e) {
            console.error("Error updating conversation list:", e);
        }
    }
};

// ============================================
// EVENT HANDLERS
// ============================================
const EventHandlers = {
    // Initialize all event handlers
    init() {
        this.initConversationListClick();
        this.initUserOverlayClick();
        this.initSendButton();
        this.initUserPanelToggle();
    },

    // Handle click on conversation entry
    initConversationListClick() {
        const conversationList = document.getElementById('conversation-list-js');
        if (conversationList) {
            conversationList.addEventListener('click', (e) => {
                const entry = e.target.closest('.conv-entry');
                if (!entry) return;
                const id = entry.getAttribute('data-convid');
                if (id) {
                    ConversationLoader.selectConversationById(parseInt(id));
                }
            });
        }
    },

    // Handle click on user in overlay
    initUserOverlayClick() {
        // Sử dụng event delegation để handle dynamic users
        document.addEventListener('click', (e) => {
            const userItem = e.target.closest('.overlay-user');
            if (!userItem) return;

            const convIdAttr = userItem.getAttribute('data-convid');
            const userId = userItem.getAttribute('data-id');
            const name = userItem.getAttribute('data-username');
            const avatar = userItem.getAttribute('data-avatar');

            if (convIdAttr) {
                // Đã có conversation
                ConversationLoader.selectConversationById(parseInt(convIdAttr));
            } else {
                // Chưa có conversation - render empty
                ConversationRenderer.renderEmptyConversation(userId, name, avatar);
            }

            // Close overlay
            const overlay = document.getElementById('user-overlay');
            if (overlay) {
                overlay.classList.remove('visible');
            }
        });
    },

    // Handle send button click
    initSendButton() {
        const sendButton = document.getElementById("sendButton");
        if (sendButton) {
            sendButton.addEventListener("click", (event) => {
                event.preventDefault();
                this.handleSendMessage();
            });
        }

        // Handle Enter key
        const chatInput = document.getElementById("chat-input-js");
        if (chatInput) {
            chatInput.addEventListener("keypress", (e) => {
                if (e.key === 'Enter' && !e.shiftKey) {
                    e.preventDefault();
                    this.handleSendMessage();
                }
            });
        }
    },

    // Handle sending message
    handleSendMessage() {
        const userElement = document.getElementById("chat-header-name");
        if (!userElement) return;

        const conversationId = userElement.getAttribute("data-convid");
        const isFirstChat = !conversationId;
        const receiverId = userElement.getAttribute("userid");
        const senderId = userData.id;
        const user = userElement.textContent;
        const messageInput = document.getElementById("chat-input-js");
        const message = messageInput ? messageInput.value.trim() : '';

        if (!message || !receiverId) {
            return;
        }

        connection.invoke("SendMessage", conversationId, senderId, receiverId, user, message, isFirstChat)
            .catch(function (err) {
                console.error("Error sending message:", err.toString());
            });
    },

    // Initialize user panel toggle
    initUserPanelToggle() {
        const overlay = document.getElementById('user-overlay');
        const openBtn = document.getElementById('open-user-panel');
        const closeBtn = document.getElementById('close-user-panel');

        if (openBtn) {
            openBtn.addEventListener('click', () => {
                if (overlay) overlay.classList.add('visible');
            });
        }

        if (closeBtn) {
            closeBtn.addEventListener('click', () => {
                if (overlay) overlay.classList.remove('visible');
            });
        }
    }
};

// ============================================
// INITIALIZATION
// ============================================
(async function init() {
    // Tìm conversation đang active từ server-side rendering
    const activeEntry = document.querySelector('.conv-entry.active');
    const activeConvId = activeEntry ? activeEntry.getAttribute('data-convid') : null;

    // Load conversations từ API
    await ConversationLoader.loadConversations();

    // Nếu có conversation active từ server, render lại để sync data
    if (activeConvId && conversationData.length > 0) {
        const activeConv = conversationData.find(c => c.id === parseInt(activeConvId));
        if (activeConv) {
            ConversationRenderer.renderConversation(activeConv);
        }
    }

    // Initialize SignalR
    connection.on("ReceiveMessage", function (id, senderId, receiverId, user, message, isFirstChat) {
        SignalRHandler.handleReceiveMessage(id, senderId, receiverId, user, message, isFirstChat);
    });

    connection.start().catch(function (err) {
        console.error("SignalR connection error:", err.toString());
    });

    // Initialize event handlers
    EventHandlers.init();
})();