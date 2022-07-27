from colorama import init, Fore, Back, Style
init()

# debug utils
def verbose(msg=""):
    print(msg)
def info(msg = "", topic = " INFO "):
    print("%s%s%s%s %s" % (Back.WHITE, Fore.BLACK, topic, Style.RESET_ALL, msg))
def warn(msg = "", topic = " WARNING "):
    print("%s%s%s%s %s" % (Back.YELLOW, Fore.BLACK, topic, Style.RESET_ALL, msg))
def err(msg = "", topic = " ERROR "):
    print("%s%s%s%s %s" % (Back.RED, Fore.BLACK, topic, Style.RESET_ALL, msg))
def ok(msg = "", topic = " O ", nbSpace = 0):
    print("%*s%s%s%s %s" % (nbSpace, Back.GREEN, Fore.BLACK, topic, Style.RESET_ALL, msg))
def notOk(msg = "", topic = " X ", nbSpace = 0):
    print("%*s%s%s%s %s" % (nbSpace, Back.RED, Fore.BLACK, topic, Style.RESET_ALL, msg))