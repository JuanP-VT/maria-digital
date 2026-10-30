import { processMessage } from './process'
import { retryDelaySeconds } from './retry'

export interface Env {
  API_BASE_URL: string
  UPLOAD_WEBHOOK_SECRET: string
}

export default {
  // Each message is settled on its own, so one failing object never replays the rest of the batch.
  // Nothing is created at module load and nothing is kept between invocations: a cold start only
  // costs the isolate boot.
  async queue(batch: MessageBatch<unknown>, env: Env): Promise<void> {
    const configured = Boolean(env.API_BASE_URL) && Boolean(env.UPLOAD_WEBHOOK_SECRET)
    if (!configured) console.error('API_BASE_URL or UPLOAD_WEBHOOK_SECRET is not set')

    await Promise.all(
      batch.messages.map(async (message) => {
        try {
          if (!configured) {
            message.retry({ delaySeconds: retryDelaySeconds(message.attempts) })
            return
          }
          const outcome = await processMessage(message.body, message.attempts, {
            fetch,
            apiBaseUrl: env.API_BASE_URL,
            secret: env.UPLOAD_WEBHOOK_SECRET,
            now: Date.now,
            random: Math.random,
          })
          if (outcome.action === 'ack') message.ack()
          else message.retry({ delaySeconds: outcome.delaySeconds })
        } catch (error) {
          console.error('Unexpected error while processing a message', error)
          message.retry({ delaySeconds: retryDelaySeconds(message.attempts) })
        }
      }),
    )
  },
} satisfies ExportedHandler<Env>
