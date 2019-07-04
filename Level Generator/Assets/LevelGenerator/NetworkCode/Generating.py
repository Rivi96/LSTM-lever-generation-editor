import numpy
import math
from keras.models import Sequential
from keras.layers import Dense
from keras.layers import Dropout
from keras.layers import LSTM
from keras.callbacks import ModelCheckpoint
from keras.utils import np_utils

# Zmiana prawdopodobieństwa wystąpienia konkretnych tilów przez określione współczynniki
def ChangeTilesProbabilities(tiles):
	for i in range(tilesNumber):
		tiles[0][i] += tileCoefficient[i]

# Parametry sieci wczytywane z pliku
generateFilePath = "../Files/NetworkFiles/GenerateFile.txt"
generateFile = open(generateFilePath)
parametersFile = "../Files/" + generateFile.readline().rstrip('\n') + ".txt"
seed = int(generateFile.readline())
tilesNumber = int(generateFile.readline())
tileCoefficient = []
for i in range(0, tilesNumber, 1):
	tileCoefficient.insert(i, float(generateFile.readline()))
parameters = open(parametersFile)
sequenceLength = int(parameters.readline())
units = int(parameters.readline())
parameters.readline()
weightsFileName = "../Files/NetworkFiles/" + parameters.readline().rstrip('\n')
savedLevelsFilePath = "../Files/" + parameters.readline() + ".txt"
savedLevelsFile = open(savedLevelsFilePath)
tiles = savedLevelsFile.readline().rstrip('\n')
levelLength = int(savedLevelsFile.readline())

# Tworzenie listy wszystkich symboli tilów
# Wartość jest symbolem, index jest liczbą opisującą element w czasie nauki
tilesSymbols = sorted(list(set(tiles)))

# Podział poziomów
allTilesNumber = len(tiles)
tilesSymbolsNumber = len(tilesSymbols)
tilesInput = []
dividedLevels = []
dividedTiles = ""
number = 0
for i in range(0, allTilesNumber + 1, 1):

	if i % levelLength == 0 and i != 0:
		number = number + 1
		dividedLevels.append(dividedTiles)
		dividedTiles = ""

	if i != allTilesNumber:
		dividedTiles += tiles[i]
		
# Stworzenie sekwencji wejściowych tilów
for k in range(0, len(dividedLevels), 1):
	for i in range(levelLength - sequenceLength, levelLength - sequenceLength + 1, 1):
		
		# Sekwencja wejścia
		inputSequence = ""
		for j in range(0, sequenceLength, 1):
			inputSequence += dividedLevels[k][j + i]

		inputSequenceRow = []
		for j in range(0, len(inputSequence), 1):
			index = tilesSymbols.index(inputSequence[j])
			inputSequenceRow.append(index)
		tilesInput.append(inputSequenceRow)

numberOfSequences = len(tilesInput)

# Wypisanie informacji
print("All tiles: ", allTilesNumber)
print("Tiles types: ", tilesSymbolsNumber)
print("Sequence length: ", sequenceLength)

# Reshape inputu na (ilość sekwencji, długośc sekwencji, numer tila)
inputX = numpy.reshape(tilesInput, (numberOfSequences, sequenceLength, 1))

# Tworzenie modelu sieci
model = Sequential()
model.add(LSTM(units, input_shape=(inputX.shape[1], inputX.shape[2]), return_sequences=True))
model.add(Dropout(0.2))
model.add(LSTM(units))
model.add(Dropout(0.2))
model.add(Dense(tilesNumber))

# Wczytywanie pliku z zapisanymi wagami
model.load_weights(weightsFileName + ".hdf5")
model.compile(optimizer='adam', loss='categorical_crossentropy')

# Generowanie poziomu zaczyna się od ostatniej sekwencji poziomu
lastLevel = tilesInput[seed - 1]

# Plik przechowujący wygenerowane wyjście
outputFile = open("../Files/NetworkFiles/OutputFile.txt", "w+")

# Pętla generująca kolejne tile i dodająca je do sekwencji
for i in range(levelLength):

	# Przygotowanie wejścia
	x = numpy.reshape(lastLevel, (1, sequenceLength, 1))
	x = x / tilesSymbolsNumber

	# Predykcja następnego tila za pomocą modelu
	nextTileToPrediction = model.predict(x)
	nextTileToOutput = nextTileToPrediction

	# Dodanie tila do sekwencji i jej przesunięcie
	index = numpy.argmax(nextTileToPrediction)
	lastLevel.append(index)
	lastLevel = lastLevel[1:len(lastLevel)]

	# Zwrócenie poziomu określonego przez seed
	# Zmiana współczynników tila przekazywanego do pliku wyjścia
	ChangeTilesProbabilities(nextTileToOutput)

	# Wywołanie funkcji argmax
	index = numpy.argmax(nextTileToOutput)
	
	# Znalezienie symbolu przypisanego do tila
	nextTileSymbol = tilesSymbols[index]
	outputFile.write(nextTileSymbol)
	print(nextTileSymbol, end="")

print("")
input("Press ENTER to exit")