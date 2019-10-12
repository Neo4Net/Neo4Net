/*
 * Copyright (c) 2002-2019 "Neo4j,"
 * Neo4j Sweden AB [http://neo4j.com]
 *
 * This file is part of Neo4j.
 *
 * Neo4j is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
namespace Org.Neo4j.Adversaries.fs
{

	using DefaultFileSystemAbstraction = Org.Neo4j.Io.fs.DefaultFileSystemAbstraction;
	using StoreFileChannel = Org.Neo4j.Io.fs.StoreFileChannel;

	/// <summary>
	/// File system abstraction that behaves exactly like <seealso cref="DefaultFileSystemAbstraction"/> <b>except</b> instead of
	/// default <seealso cref="FileChannel"/> implementation <seealso cref="AdversarialFileChannel"/> will be used.
	/// 
	/// This abstraction should be used in cases when it's desirable to have default file system implementation
	/// and only verify handling of inconsistent channel operations.
	/// Otherwise consider <seealso cref="AdversarialFileSystemAbstraction"/> since it should produce more failure cases.
	/// </summary>
	public class AdversarialChannelDefaultFileSystemAbstraction : DefaultFileSystemAbstraction
	{
		 private readonly RandomAdversary _adversary;

		 public AdversarialChannelDefaultFileSystemAbstraction() : this(new RandomAdversary(0.5, 0.0, 0.0))
		 {
		 }

		 public AdversarialChannelDefaultFileSystemAbstraction( RandomAdversary adversary )
		 {
			  this._adversary = adversary;
		 }

		 protected internal override StoreFileChannel GetStoreFileChannel( FileChannel channel )
		 {
			  return AdversarialFileChannel.Wrap( base.GetStoreFileChannel( channel ), _adversary );
		 }
	}

}