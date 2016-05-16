import requests

class DumplingService:
    _dumplingUri = 'http://dotnetrp.azurewebsites.net';

    @staticmethod
    def SayHelloAs(username):
        hello_url = DumplingService._dumplingUri + '/dumpling/test/hi/im/%s'%(username)
        response = requests.get(hello_url)

        return response.content

    @staticmethod
    def UploadZip(username, origin, filepath):
        upload_url = DumplingService._dumplingUri + '/dumpling/store/chunk/%s/%s/0/0'%(username, origin);
        files = {'file': open(file, 'rb')}
        response = requests.post(upload_url, files = files)

        return response.content

if __name__ == '__main__':
    username = 'bryanar'
    origin = 'ubuntu'
    file = 'C:/temp/dumps/ubuntu/projectk-24025-00-amd64chk_00AB.zip'

    print DumplingService.SayHelloAs('bryanar')
    print DumplingService.UploadZip(username, origin, file)
