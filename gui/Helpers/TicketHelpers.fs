namespace CIMSystemGUI.Components

open CIMSystemGUI.Models

module TicketHelpers =

    let generateHtmlTicket (ticketInfo: TicketInfo) =
        match CIMSystemGUI.Services.HtmlTicketGenerator.saveTicketAsHtml ticketInfo with
        | Result.Ok filename -> Result.Ok filename
        | Result.Error htmlError -> Result.Error htmlError

    let createSuccessMessage (msg: string) (ticketInfo: TicketInfo) (htmlResult: Result<string, string>) =
        match htmlResult with
        | Result.Ok filename -> $"{msg}\n🎫 Ticket created: {filename}\n📋 Ticket ID: {ticketInfo.TicketId}"
        | Result.Error htmlError ->
            $"{msg}\n⚠️ Ticket created but HTML generation failed: {htmlError}\n📋 Ticket ID: {ticketInfo.TicketId}"

    let handleTicketGeneration (msg: string) (ticketInfo: TicketInfo) =
        // Check if ticket exists and try to generate HTML
        match CIMSystemGUI.Services.TicketService.getTicketInfo ticketInfo.TicketId with
        | Some(_, false) ->
            // Try to generate HTML ticket
            let htmlResult = generateHtmlTicket ticketInfo
            createSuccessMessage msg ticketInfo htmlResult
        | _ -> $"{msg}\n📋 Ticket ID: {ticketInfo.TicketId}"
