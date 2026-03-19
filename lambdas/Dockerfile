FROM public.ecr.aws/lambda/python:3.12

COPY requirements.txt ${LAMBDA_TASK_ROOT}/
RUN pip install --no-cache-dir -r requirements.txt

COPY ./src ${LAMBDA_TASK_ROOT}/src

CMD ["src.ortho_transform.handler.lambda_handler"]