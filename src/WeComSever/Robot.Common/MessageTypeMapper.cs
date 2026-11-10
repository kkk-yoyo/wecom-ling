using System.Collections.Generic;

namespace Robot.Common;

public class MessageTypeMapper
{
	public static Dictionary<int, string> CodeMappings = new Dictionary<int, string>
	{
		{ 11087, "QRCode" },
		{ 11025, "LoginSuccess" },
		{ 11026, "Logout" },
		{ 11046, "TextMessage" },
		{ 11047, "ImageMessage" },
		{ 11048, "SpeechMessage" },
		{ 11049, "AddFriendsMessage" },
		{ 11050, "CardMessage" },
		{ 11051, "VideoMessage" },
		{ 11052, "EmojiMessage" },
		{ 11053, "PositionMessage" },
		{ 11058, "SystemMessage" },
		{ 11059, "RecallMessage" },
		{ 11095, "QRCodeMeansMessage" },
		{ 11060, "OtherMessage" },
		{ 11054, "LinkMessage" },
		{ 11055, "FileMessage" },
		{ 11056, "AppletMessage" },
		{ 11057, "TransferMessage" },
		{ 11061, "OtherApplicationMessage" },
		{ 11102, "NewFriendsAdded" },
		{ 11103, "FriendDeletion" },
		{ 11098, "NewGroupMembers" },
		{ 11099, "GroupMembersExit" },
		{ 11100, "GroupCreation " },
		{ 11101, "ExitGroup" },
		{ 11088, "WindowTransformation" },
		{ 11091, "SwitchChat" },
		{ 100, "FileDownloadFinish" },
		{ 1, "Heartbeat" }
	};
}
