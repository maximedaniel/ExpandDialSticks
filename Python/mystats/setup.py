import setuptools

with open("README.md", "r") as fh:
    long_description = fh.read()

setuptools.setup(
     name='mystats',  
     version='0.1',
     packages=setuptools.find_packages(),
     author="Maxime DANIEL",
     author_email="maxaxeldaniel@gmail.com",
     description="My HCI statistical tool",
     long_description=long_description,
     long_description_content_type="text/markdown",
     url="",
     classifiers=[
         "Programming Language :: Python :: 3",
         "License :: OSI Approved :: MIT License",
         "Operating System :: OS Independent",
     ],
 )