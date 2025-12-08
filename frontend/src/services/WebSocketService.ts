import * as signalR from '@microsoft/signalr';

interface ConsultationMessage {
  id: number;
  consultationId: number;
  senderId: string;
  senderName?: string;
  text?: string;
  status: string;
  sentAt: string;
}

export class WebSocketService {
  private consultationConnection: signalR.HubConnection | null = null;
  private notificationConnection: signalR.HubConnection | null = null;
  private apiUrl = import.meta.env.VITE_API_URL?.replace('/api', '') || 'http://localhost:5136';
  private token: string | null = null;

  // Callbacks for events
  private onMessageReceived: ((message: ConsultationMessage) => void) | null = null;
  private onConsultationAccepted: ((data: any) => void) | null = null;
  private onConsultationCompleted: ((data: any) => void) | null = null;
  private onConsultationUpdated: ((data: any) => void) | null = null;
  private onNewConsultationRequest: ((data: any) => void) | null = null;
  private onNewNotification: ((notification: any) => void) | null = null;
  private onConnected: ((data: any) => void) | null = null;
  private onError: ((message: string) => void) | null = null;

  constructor() {
    this.token = localStorage.getItem('token');
  }

  // Initialize consultation hub connection
  async initializeConsultationConnection(): Promise<void> {
    try {
      // Refresh token from localStorage
      this.token = localStorage.getItem('token');
      
      this.consultationConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${this.apiUrl}/hubs/consultations`, {
          accessTokenFactory: () => this.token || '',
        })
        .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
        .configureLogging(signalR.LogLevel.Information)
        .build();

      this.consultationConnection.on('ReceiveConsultationMessage', (message: ConsultationMessage) => {
        console.log('Received message via WebSocket:', message);
        if (this.onMessageReceived) {
          this.onMessageReceived(message);
        }
      });

      this.consultationConnection.on('ConsultationAccepted', (data: any) => {
        console.log('Consultation accepted via WebSocket:', data);
        if (this.onConsultationAccepted) {
          this.onConsultationAccepted(data);
        }
      });

      this.consultationConnection.on('ConsultationDeclined', (data: any) => {
        console.log('Consultation declined via WebSocket:', data);
      });

      this.consultationConnection.on('ConsultationCompleted', (data: any) => {
        console.log('Consultation completed via WebSocket:', data);
        if (this.onConsultationCompleted) {
          this.onConsultationCompleted(data);
        }
      });

      this.consultationConnection.on('PaymentAccepted', (data: any) => {
        console.log('Payment accepted via WebSocket:', data);
        if (this.onConsultationUpdated) {
          this.onConsultationUpdated(data);
        }
      });

      this.consultationConnection.on('PriceUpdated', (data: any) => {
        console.log('Price updated via WebSocket:', data);
        if (this.onConsultationUpdated) {
          this.onConsultationUpdated(data);
        }
      });

      this.consultationConnection.on('PriceRejected', (data: any) => {
        console.log('Price rejected via WebSocket:', data);
        if (this.onConsultationUpdated) {
          this.onConsultationUpdated(data);
        }
      });

      this.consultationConnection.on('NewConsultationRequest', (data: any) => {
        console.log('New consultation request via WebSocket:', data);
        if (this.onNewConsultationRequest) {
          this.onNewConsultationRequest(data);
        }
      });

      this.consultationConnection.onreconnected((connectionId: string | undefined) => {
        console.log('WebSocket reconnected, connectionId:', connectionId);
      });

      this.consultationConnection.onreconnecting((error: Error | undefined) => {
        console.log('WebSocket reconnecting...', error);
      });

      this.consultationConnection.onclose((error: Error | undefined) => {
        console.log('WebSocket closed:', error);
      });

      await this.consultationConnection.start();
      console.log('Consultation WebSocket connected');
      
      if (this.onConnected) {
        this.onConnected({ connectionId: this.consultationConnection.connectionId });
      }
    } catch (error) {
      console.error('Failed to connect to consultation hub:', error);
      if (this.onError) {
        this.onError(`Failed to connect: ${error}`);
      }
      throw error;
    }
  }

  // Initialize notification hub connection
  async initializeNotificationConnection(): Promise<void> {
    try {
      this.notificationConnection = new signalR.HubConnectionBuilder()
        .withUrl(`${this.apiUrl}/hubs/notifications`, {
          accessTokenFactory: () => this.token || '',
          withCredentials: true,
        })
        .withAutomaticReconnect([0, 0, 0, 0, 0, 5000, 5000, 5000])
        .build();

      this.notificationConnection.on('ReceiveNotification', (notification: any) => {
        console.log('Received notification via WebSocket:', notification);
        if (this.onNewNotification) {
          this.onNewNotification(notification);
        }
      });

      await this.notificationConnection.start();
      console.log('Notification WebSocket connected');
    } catch (error) {
      console.error('Failed to connect to notification hub:', error);
      if (this.onError) {
        this.onError(`Failed to connect: ${error}`);
      }
    }
  }

  // Send message through WebSocket
  async sendConsultationMessage(
    consultationId: number,
    message: string,
    senderName: string,
    senderRole: string
  ): Promise<void> {
    if (!this.consultationConnection || this.consultationConnection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('WebSocket not connected');
    }

    try {
      await this.consultationConnection.invoke('SendConsultationMessage', consultationId, message, senderName, senderRole);
      console.log('Message sent via WebSocket');
    } catch (error) {
      console.error('Failed to send message:', error);
      throw error;
    }
  }

  // Join a consultation room
  async joinConsultation(consultationId: number): Promise<void> {
    if (!this.consultationConnection || this.consultationConnection.state !== signalR.HubConnectionState.Connected) {
      throw new Error('WebSocket not connected');
    }

    try {
      await this.consultationConnection.invoke('JoinConsultation', consultationId);
      console.log(`Joined consultation ${consultationId}`);
    } catch (error) {
      console.error('Failed to join consultation:', error);
      throw error;
    }
  }

  // Leave a consultation room
  async leaveConsultation(consultationId: number): Promise<void> {
    if (!this.consultationConnection || this.consultationConnection.state !== signalR.HubConnectionState.Connected) {
      return;
    }

    try {
      await this.consultationConnection.invoke('LeaveConsultation', consultationId);
      console.log(`Left consultation ${consultationId}`);
    } catch (error) {
      console.error('Failed to leave consultation:', error);
    }
  }

  // Register callbacks
  onMessageReceivedCallback(callback: (message: ConsultationMessage) => void): void {
    this.onMessageReceived = callback;
  }

  onConsultationAcceptedCallback(callback: (data: any) => void): void {
    this.onConsultationAccepted = callback;
  }

  onConsultationCompletedCallback(callback: (data: any) => void): void {
    this.onConsultationCompleted = callback;
  }

  onConsultationUpdatedCallback(callback: (data: any) => void): void {
    this.onConsultationUpdated = callback;
  }

  onNewConsultationRequestCallback(callback: (data: any) => void): void {
    this.onNewConsultationRequest = callback;
  }

  onNewNotificationCallback(callback: (notification: any) => void): void {
    this.onNewNotification = callback;
  }

  onConnectedCallback(callback: (data: any) => void): void {
    this.onConnected = callback;
  }

  onErrorCallback(callback: (message: string) => void): void {
    this.onError = callback;
  }

  // Check connection status
  isConsultationConnected(): boolean {
    return this.consultationConnection?.state === signalR.HubConnectionState.Connected;
  }

  isNotificationConnected(): boolean {
    return this.notificationConnection?.state === signalR.HubConnectionState.Connected;
  }

  // Disconnect
  async disconnect(): Promise<void> {
    if (this.consultationConnection) {
      await this.consultationConnection.stop();
      console.log('Consultation WebSocket disconnected');
    }
    if (this.notificationConnection) {
      await this.notificationConnection.stop();
      console.log('Notification WebSocket disconnected');
    }
  }

  // Update token (call this when token changes)
  updateToken(newToken: string): void {
    this.token = newToken;
    localStorage.setItem('token', newToken);
  }
}

// Export singleton instance
export const webSocketService = new WebSocketService();
