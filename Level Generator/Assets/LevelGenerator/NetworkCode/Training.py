import numpy
from keras.models import Sequential
from keras.layers import Dense
from keras.layers import Dropout
from keras.layers import LSTM
from keras.callbacks import ModelCheckpoint
from keras.utils import np_utils

# Parametry sieci wczytywane z pliku
trainFile = "../Files/NetworkFiles/TrainFile.txt"
fileName = open(trainFile).readline()
parametersFile = "../Files/" + fileName + ".txt"
parameters = open(parametersFile)
sequenceLength = int(parameters.readline())
units = int(parameters.readline())
epochs = int(parameters.readline())
weightsFileName = parameters.readline().rstrip('\n')
savedLevelsFilePath = "../Files/" + parameters.readline() + ".txt"
savedLevelsFile = open(savedLevelsFilePath)
tiles = savedLevelsFile.readline().rstrip('\n')
levelLength = int(savedLevelsFile.readline())

# Tworzenie listy wszystkich symboli tilów
# Wartość jest symbolem, index jest liczbą opisującą element w czasie nauki
tilesSymbols = sorted(list(set(tiles)))

# Zmienne
allTilesNumber = len(tiles)
tilesSymbolsNumber = len(tilesSymbols)
tilesInput = []
tilesOutput = []

# Podział poziomów
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
		
# Stworzenie sekwencji wejściowych tilów i ich wyjść
for k in range(0, len(dividedLevels), 1):
	for i in range(0, levelLength - sequenceLength, 1):
		
		# Sekwencja wejścia
		inputSequence = ""
		for j in range(0, sequenceLength, 1):
			inputSequence += dividedLevels[k][j + i]

		inputSequenceRow = []
		for j in range(0, len(inputSequence), 1):
			index = tilesSymbols.index(inputSequence[j])
			inputSequenceRow.append(index)
		tilesInput.append(inputSequenceRow)
		
		# Wyjście
		outputSequence = dividedLevels[k][i + sequenceLength]
		index = tilesSymbols.index(outputSequence)
		tilesOutput.append(index)

numberOfSequences = len(tilesInput)

# Wypisanie informacji
print("All tiles: ", allTilesNumber)
print("Tiles types: ", tilesSymbolsNumber)
print("Sequence length: ", sequenceLength)
print("Number of sequences: ", numberOfSequences)

# Reshape inputu na (ilość sekwencji, długośc sekwencji, numer tila)
inputX = numpy.reshape(tilesInput, (numberOfSequences, sequenceLength, 1))
# Normalizacja inputu
inputX = inputX / tilesSymbolsNumber
# Kodowanie wyjścia na kod 1 z n
outputY = np_utils.to_categorical(tilesOutput)

# Zapis ostatnich i najlepszych wag do pliku
weightsFilepath = "../Files/NetworkFiles/" + weightsFileName + ".hdf5"
callbacks = [ModelCheckpoint(weightsFilepath, monitor='loss', verbose=1, save_best_only=True, mode='min')]

# Tworzenie modelu sieci
model = Sequential()
model.add(LSTM(units, input_shape=(inputX.shape[1], inputX.shape[2]), return_sequences=True))
model.add(Dropout(0.2))
model.add(LSTM(units))
model.add(Dropout(0.2))
model.add(Dense(outputY.shape[1], activation='softmax'))
model.compile(optimizer='adam', loss='categorical_crossentropy')

# Trenowanie sieci
input("Press ENTER to start training")
model.fit(inputX, outputY, epochs=epochs, batch_size=20, callbacks=callbacks)
input("Press ENTER to exit")