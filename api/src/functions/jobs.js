const { app } = require('@azure/functions');

app.http('jobs/run-now', {
    methods: ['POST'],
    authLevel: 'anonymous',
    handler: async (request, context) => {
        context.log(`HTTP function processed request for URL: ${request.url}`);

        // Parse JSON body
        const body = await request.json().catch(() => null);
        if (!body || typeof body.job_parameters !== 'object') {
            return {
                status: 400,
                body: 'Invalid or missing job_parameters object.'
            };
        }

        const job_parameters = body.job_parameters;

        // Log the object
        context.log('Received job_parameters:', JSON.stringify(job_parameters));

        return {
            status: 200,
            body: 'Job parameters received and logged successfully.'
        };
    }
});