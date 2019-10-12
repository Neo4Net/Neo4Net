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
namespace Neo4Net.Register
{
	/// <summary>
	/// Collection of common register types.
	/// </summary>
	public interface Register
	{
	}

	 public interface Register_CopyableDoubleLongRegister : Register_DoubleLong_Copyable, Register_DoubleLong_Out
	 {
	 }

	 public interface Register_DoubleLongRegister : Register_DoubleLong_In, Register_CopyableDoubleLongRegister
	 {
	 }

	 public interface Register_DoubleLong
	 {
	 }

	  public interface Register_DoubleLong_In
	  {
			long ReadFirst();

			long ReadSecond();
	  }

	  public interface Register_DoubleLong_Copyable
	  {
			void CopyTo( Register_DoubleLong_Out target );

			bool HasValues( long first, long second );
	  }

	  public interface Register_DoubleLong_Out
	  {
			void Write( long first, long second );

			void Increment( long firstDelta, long secondDelta );
	  }

	 public interface Register_System.Nullable<long>
	 {
	 }

	  public interface Register_System_In
	  {
			long Read();
	  }

	  public interface Register_System_Out
	  {
			void Write( long value );

			long Increment( long delta );
	  }

	 public interface Register_Int
	 {
	 }

	  public interface Register_Int_In
	  {
			int Read();
	  }

	  public interface Register_Int_Out
	  {
			void Write( int value );

			int Increment( int delta );
	  }

	 public interface Register_Object
	 {

	 }

	  public interface Register_Object_In<T>
	  {
			T Read();
	  }

	  public interface Register_Object_Out<T>
	  {
			void Write( T value );
	  }

}