//Brouwer, Evyn 100702628
#define EXPORT_API __declspec(dllexport)

#include <String>
extern "C"
{
	//a simple vector2
	struct Vector2Int {
		int x, y;
	};
	//generate a size of the room
	Vector2Int EXPORT_API generateRoomSize(int minSize, int maxSize) {
		Vector2Int data;
		data.x = rand()%(maxSize-minSize)+minSize;
		data.y = rand() % (maxSize - minSize) + minSize;
		return data;
	}
	Vector2Int EXPORT_API generateRoomPos(int minPos, int maxPosX, int maxPosY) {
		Vector2Int data;
		data.x = rand() % (maxPosX - minPos) + minPos;
		data.y = rand() % (maxPosY - minPos) + minPos;
		return data;
	}

	//backup DLL code
	//Vector2Int EXPORT_API generateRoomSize(int minSize, int maxSize) {
	//	Vector2Int data;
	//	data.x = 0;
	//	data.y = 0;
	//	return data;
	//}

	/*
	this was a much more complicated process that I won't have time to complete all room generation would be handled here
	*/
	
	//simple room struct
	struct Room {
		int px = 0, py = 0;//position
		int sx = 0, sy = 0;//size
	};
	/*
	we use this function to see if rooms are overlapping +1 on each side
	*/
	bool checkCollision(Room* roomData, int dataSize,Room r, int*rooms, int sizeX, int sizeY) {
		for (int i = 0; i < dataSize; i++)
		{
			bool xCollision = false, yCollision = false;

			xCollision = r.px - 1 + r.sx + 1 >= roomData[i].px - 1 && roomData[i].px - 1 + roomData[i].sx + 1 >= r.px - 1;
			yCollision = r.py - 1 + r.sy + 1 >= roomData[i].py - 1 && roomData[i].py - 1 + roomData[i].sy + 1 >= r.py - 1;
			if (xCollision && yCollision)
			{
				return true;
			}
		}
		return false;
	}
	/*
	we attempt to place a room 50 times before giving up, if it was infinite we could loop forever in some cases as it is recursive
	*/
	int* placeRoom(Room* roomData, int dataSize, Room r, int* rooms, int sizeX, int sizeY, int counter) {
		counter++;
		int xp, yp;
		xp = rand() % sizeX - r.sx;
		if (xp < 0)
			xp = 0;
		yp = rand() % sizeY - r.sy;
		if (yp < 0)
			yp = 0;
		if (counter > 50) {
			r.sx = 0;
			r.sy = 0;
			r.px = 0;
			r.py = 0;
		}
		if (checkCollision(roomData, dataSize, r, rooms, sizeX, sizeY))
			return placeRoom(roomData, dataSize, r, rooms, sizeX, sizeY,counter);
		else
			for (int i = r.px; i < r.px + r.sx; i++) {
				for (int n = r.py; n < r.py + r.sy; n++) {
					rooms[i, n] = 1;
				}
			}
		return rooms;
	}
	
	
	//this function will return a string that we will use in unity
	std::string EXPORT_API generateRandomDungeon(int sizeX, int sizeY,int minRoom, int maxRoom, int minSize, int maxSize) {
		int RoomCount = rand() % (maxRoom - minRoom) + minRoom;//make a number of rooms
		int* rooms = new int[sizeX,sizeY];//base a 2d grid of the size given
		Room* roomData = new Room[RoomCount];//an array of rooms
		for (unsigned i = 0; i < RoomCount; i++) {
			roomData[i] = Room();
		}
		for (unsigned i = 0; i < sizeX; i++) {
			for (unsigned n = 0; n < sizeY; n++) {
				rooms[i, n] = 0;
			}
		}

		for (unsigned i = 0; i < RoomCount; i++) {
			int roomSizeX = rand() % (maxSize - minSize) + minSize;
			int roomSizeY = rand() % (maxSize - minSize) + minSize;
			Room temp;
			temp.sx = roomSizeX;
			temp.sy = roomSizeY;
			rooms = placeRoom(roomData,RoomCount,temp,rooms,sizeX,sizeY,0);
		}
		std::string temp;
		return temp;
	}
	
}