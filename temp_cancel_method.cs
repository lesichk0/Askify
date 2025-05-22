        public async Task<bool> CancelConsultationAsync(int id)
        {
            var consultation = await _unitOfWork.Consultations.GetByIdAsync(id);
            if (consultation == null) return false;
            
            // Store the previous status to check if this was a decline operation
            string previousStatus = consultation.Status ?? "";
            string previousExpertId = consultation.ExpertId ?? "";

            consultation.Status = "Cancelled";
            _unitOfWork.Consultations.Update(consultation);
            var result = await _unitOfWork.CompleteAsync();
            
            // If result is successful and this was a decline operation (had an expert and was Pending)
            if (result && previousStatus == "Pending" && !string.IsNullOrEmpty(previousExpertId))
            {
                // Get expert's name for the notification
                var expert = await _unitOfWork.Users.GetByIdAsync(previousExpertId);
                string expertName = expert?.FullName ?? "The expert";
                
                // Notify the user that the expert declined
                await _notificationService.CreateNotificationAsync(
                    consultation.UserId,
                    "ConsultationDeclined",
                    consultation.Id,
                    $"{expertName} has declined your consultation request");
            }
            
            return result;
        }
