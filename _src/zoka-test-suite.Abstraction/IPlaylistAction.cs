using System;
using System.Collections.Generic;
using System.Text;
using Zoka.ZScript;

namespace Zoka.TestSuite.Abstraction
{
	/// <summary>Interface of the Test playlist action</summary>
	public interface IPlaylistAction
	{
		/// <summary>Name of the action</summary>
		string Name { get; }

		/// <summary>Id of the action</summary>
		string Id { get; }

		/// <summary>Description of the action</summary>
		string Description { get; }

		/// <summary>Will perform the playlist action</summary>
		int PerformAction(DataStorages _data_storages, IServiceProvider _service_provider);

	}
}
