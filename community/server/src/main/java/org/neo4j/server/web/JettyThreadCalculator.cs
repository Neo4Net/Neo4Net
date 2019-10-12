using System;

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
namespace Org.Neo4j.Server.web
{
	public class JettyThreadCalculator
	{
		 // Any higher and maxCapacity will overflow
		 public const int MAX_THREADS = 44738;

		 private int _acceptors;
		 private int _selectors;
		 private int _minThreads;
		 private int _maxThreads;
		 private int _maxCapacity;

		 public JettyThreadCalculator( int jettyMaxThreads )
		 {
			  if ( jettyMaxThreads < 1 )
			  {
					throw new System.ArgumentException( "Max threads can't be less than 1" );
			  }
			  else if ( jettyMaxThreads > MAX_THREADS )
			  {
					throw new System.ArgumentException( string.Format( "Max threads can't exceed {0:D}", MAX_THREADS ) );
			  }
			  // transactionThreads = N / 5
			  int transactionThreads = jettyMaxThreads / 5;
			  // acceptors = N / 15
			  _acceptors = Math.Max( 1, transactionThreads / 3 );
			  // selectors = N * 2 / 15
			  _selectors = Math.Max( 1, transactionThreads - _acceptors );
			  if ( jettyMaxThreads < 4 )
			  {
					_acceptors = 1;
					_selectors = 1;
			  }
			  else if ( jettyMaxThreads == 4 )
			  {
					_acceptors = 1;
					_selectors = 2;
			  }
			  else if ( jettyMaxThreads <= 8 )
			  {
					_acceptors = 2;
					_selectors = 3;
			  }
			  else if ( jettyMaxThreads <= 16 )
			  {
					transactionThreads = jettyMaxThreads / 4;
					_acceptors = Math.Max( 2, transactionThreads / 3 );
					_selectors = Math.Max( 3, transactionThreads - _acceptors );
			  }
			  // minThreads = N / 5 + 2 * N / 5
			  // max safe value for this = 5 / 3 * INT.MAX = INT.MAX
			  _minThreads = Math.Max( 2, transactionThreads ) + ( _acceptors + _selectors ) * 2;
			  // maxThreads = N + N / 5
			  // max Safe value for this = 6 / 5 * INT.MAX = INT.MAX
			  _maxThreads = Math.Max( jettyMaxThreads - _selectors - _acceptors, 8 ) + ( _acceptors + _selectors ) * 2;
			  // maxCapacity = (N - N / 5) * 60_000
			  // max safe value = 44738
			  _maxCapacity = ( _maxThreads - ( _selectors + _acceptors ) * 2 ) * 1000 * 60; // threads * 1000 req/s * 60 s
		 }

		 public virtual int Acceptors
		 {
			 get
			 {
				  return _acceptors;
			 }
		 }

		 public virtual int Selectors
		 {
			 get
			 {
				  return _selectors;
			 }
		 }

		 public virtual int MinThreads
		 {
			 get
			 {
				  return _minThreads;
			 }
		 }

		 public virtual int MaxThreads
		 {
			 get
			 {
				  return _maxThreads;
			 }
		 }

		 public virtual int MaxCapacity
		 {
			 get
			 {
				  return _maxCapacity;
			 }
		 }
	}

}