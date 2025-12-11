namespace CEMSystem.Services

open System
open System.IO
open CEMSystem.Models

module HtmlTicketGenerator =

    // Generate HTML ticket content
    let generateTicketHtml (ticketInfo: TicketInfo) =
        let bookingDate = ticketInfo.BookingDate.ToString("yyyy-MM-dd")
        let bookingTime = ticketInfo.BookingDate.ToString("HH:mm")
        let generatedTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        let seatNumber = sprintf "R%02d-S%02d" ticketInfo.SeatRow ticketInfo.SeatColumn

        sprintf
            """<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Cinema Ticket - %s</title>
    <style>
        body {
            font-family: Arial, sans-serif;
            background-color: #f0f0f0;
            margin: 0;
            padding: 20px;
            display: flex;
            justify-content: center;
            align-items: center;
            min-height: 100vh;
        }
        
        .ticket {
            background: linear-gradient(135deg, #667eea 0%%, #764ba2 100%%);
            border-radius: 15px;
            padding: 30px;
            max-width: 400px;
            width: 100%%;
            box-shadow: 0 10px 30px rgba(0,0,0,0.3);
            color: white;
            position: relative;
            overflow: hidden;
        }
        
        .ticket::before {
            content: '';
            position: absolute;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background: url('data:image/svg+xml,<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 100 100"><circle cx="50" cy="50" r="2" fill="rgba(255,255,255,0.1)"/></svg>') repeat;
            background-size: 50px 50px;
            pointer-events: none;
        }
        
        .ticket-content {
            position: relative;
            z-index: 1;
        }
        
        .header {
            text-align: center;
            margin-bottom: 25px;
            border-bottom: 2px dashed rgba(255,255,255,0.3);
            padding-bottom: 15px;
        }
        
        .cinema-name {
            font-size: 24px;
            font-weight: bold;
            margin-bottom: 5px;
            text-transform: uppercase;
            letter-spacing: 2px;
        }
        
        .ticket-type {
            font-size: 14px;
            opacity: 0.8;
            text-transform: uppercase;
        }
        
        .info-section {
            margin-bottom: 20px;
        }
        
        .info-row {
            display: flex;
            justify-content: space-between;
            margin-bottom: 10px;
            align-items: center;
        }
        
        .info-label {
            font-weight: bold;
            font-size: 14px;
            opacity: 0.9;
        }
        
        .info-value {
            font-size: 16px;
            font-weight: bold;
        }
        
        .seat-info {
            background: rgba(255,255,255,0.2);
            border-radius: 8px;
            padding: 15px;
            text-align: center;
            margin: 20px 0;
        }
        
        .seat-number {
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 5px;
        }
        
        .seat-label {
            font-size: 12px;
            opacity: 0.8;
            text-transform: uppercase;
        }
        
        .ticket-id {
            background: rgba(0,0,0,0.3);
            border-radius: 8px;
            padding: 15px;
            margin-top: 20px;
            text-align: center;
        }
        
        .ticket-id-label {
            font-size: 12px;
            opacity: 0.8;
            margin-bottom: 5px;
            text-transform: uppercase;
        }
        
        .ticket-id-value {
            font-family: 'Courier New', monospace;
            font-size: 16px;
            font-weight: bold;
            word-break: break-all;
        }
        
        .footer {
            text-align: center;
            margin-top: 25px;
            padding-top: 15px;
            border-top: 2px dashed rgba(255,255,255,0.3);
            font-size: 12px;
            opacity: 0.7;
        }
        
        @media print {
            body {
                background-color: white;
                margin: 0;
                padding: 10px;
            }
            .ticket {
                box-shadow: none;
                border: 2px solid #333;
                background: white;
                color: black;
            }
        }
    </style>
</head>
<body>
    <div class="ticket">
        <div class="ticket-content">
            <div class="header">
                <div class="cinema-name">CEM Cinema</div>
                <div class="ticket-type">Cinema Ticket</div>
            </div>
            
            <div class="info-section">
                <div class="info-row">
                    <span class="info-label">Customer:</span>
                    <span class="info-value">%s</span>
                </div>
                <div class="info-row">
                    <span class="info-label">Booking Date:</span>
                    <span class="info-value">%s</span>
                </div>
                <div class="info-row">
                    <span class="info-label">Booking Time:</span>
                    <span class="info-value">%s</span>
                </div>
            </div>
            
            <div class="seat-info">
                <div class="seat-number">%s</div>
                <div class="seat-label">Your Seat</div>
            </div>
            
            <div class="ticket-id">
                <div class="ticket-id-label">Ticket ID</div>
                <div class="ticket-id-value">%s</div>
            </div>
            
            <div class="footer">
                <p>Please present this ticket at the entrance</p>
                <p>Valid for one-time use only</p>
                <p>Generated on %s</p>
            </div>
        </div>
    </div>
</body>
</html>"""
            ticketInfo.TicketId
            ticketInfo.CustomerName
            bookingDate
            bookingTime
            seatNumber
            ticketInfo.TicketId
            generatedTime

    // Save ticket as HTML file
    let saveTicketAsHtml (ticketInfo: TicketInfo) =
        try
            let filename = $"Tickets/ticket_{ticketInfo.TicketId}.html"
            let htmlContent = generateTicketHtml ticketInfo
            File.WriteAllText(filename, htmlContent)
            Result.Ok filename
        with ex ->
            Result.Error $"Failed to save ticket as HTML: {ex.Message}"