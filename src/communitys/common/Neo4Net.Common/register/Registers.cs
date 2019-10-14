/*
 * Copyright © 2018-2020 "Neo4Net,"
 * Team NeoN [http://neo4net.com]. All Rights Reserved.
 *
 * This file is part of Neo4Net.
 *
 * Neo4Net is free software: you can redistribute it and/or modify
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
	public class Registers
	{
		 private Registers()
		 {
		 }

		 public static Register_DoubleLongRegister NewDoubleLongRegister()
		 {
			  return NewDoubleLongRegister( -1L, -1L );
		 }

//JAVA TO C# CONVERTER WARNING: 'final' parameters are ignored unless the option to convert to C# 7.2 'in' parameters is selected:
//ORIGINAL LINE: public static Register_DoubleLongRegister newDoubleLongRegister(final long initialFirst, final long initialSecond)
		 public static Register_DoubleLongRegister NewDoubleLongRegister( long initialFirst, long initialSecond )
		 {
			  return new Register_DoubleLongRegisterAnonymousInnerClass( initialFirst, initialSecond );
		 }

		 private class Register_DoubleLongRegisterAnonymousInnerClass : Register_DoubleLongRegister
		 {
			 private long _initialFirst;
			 private long _initialSecond;

			 public Register_DoubleLongRegisterAnonymousInnerClass( long initialFirst, long initialSecond )
			 {
				 this._initialFirst = initialFirst;
				 this._initialSecond = initialSecond;
			 }

			 private long first = _initialFirst;
			 private long second = _initialSecond;

			 public long readFirst()
			 {
				  return first;
			 }

			 public long readSecond()
			 {
				  return second;
			 }

			 public void copyTo( Register_DoubleLong_Out target )
			 {
				  target.Write( first, second );
			 }

			 public bool hasValues( long first, long second )
			 {
				  return this.first == first && this.second == second;
			 }

			 public void write( long first, long second )
			 {
				  this.first = first;
				  this.second = second;
			 }

			 public void increment( long firstDelta, long secondDelta )
			 {
				  this.first += firstDelta;
				  this.second += secondDelta;
			 }

			 public override string ToString()
			 {
				  return "DoubleLongRegister{first=" + first + ", second=" + second + "}";
			 }
		 }
	}

}