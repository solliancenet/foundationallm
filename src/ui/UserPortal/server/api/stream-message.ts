
import mockMessageStreamData from './mockMessageStreamData';

let mockDataIndex = 0;
let mockMessage = {};
let generationInterval = null;
let isGenerating = false;

function startGeneration() {
	isGenerating = true;
	mockMessage = mockMessageStreamData[mockDataIndex];

	generationInterval = setInterval(() => {
		mockDataIndex += 1;

		mockMessage = mockMessageStreamData[mockDataIndex];

		const lastIndex = mockMessageStreamData.length - 1;
		if (mockDataIndex >= lastIndex) {
			mockMessage = mockMessageStreamData[lastIndex];
		}
	}, 150);
}

function stopGeneration() {
	mockDataIndex = 0;
	mockMessage = {};
	isGenerating = false;
	clearInterval(generationInterval);
}

export default defineEventHandler(async (event) => {
	if (!isGenerating) {
		startGeneration();
	}

	const message = JSON.parse(JSON.stringify(mockMessage));

	if (mockDataIndex >= mockMessageStreamData.length - 1) {
		stopGeneration();
	}

	return message;
});
