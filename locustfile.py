from locust import HttpUser, task, between

class EvChargingUser(HttpUser):
    # Кожен юзер робить запит, чекає 1-3 секунди і повторює
    wait_time = between(1, 3)

    @task
    def load_transactions(self):
        # Смикаємо твій ендпоінт
        self.client.get("/api/Transactions/all")

        