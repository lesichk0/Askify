import React from 'react';
import { Link } from 'react-router-dom';

const Footer: React.FC = () => {
  return (
    <footer className="bg-gray-800 text-gray-300">
      <div className="container mx-auto px-4 py-8">
        <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
          <div>
            <h3 className="text-lg font-semibold text-yellow-400 mb-4">Askify</h3>
            <p className="text-sm">
              Your platform for expert consultations and knowledge sharing.
            </p>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-yellow-400 mb-4">Quick Links</h3>
            <ul className="space-y-2 text-sm">
              <li><Link to="/" className="hover:text-yellow-300 transition">Home</Link></li>
              <li><Link to="/consultations" className="hover:text-yellow-300 transition">Consultations</Link></li>
              <li><Link to="/experts" className="hover:text-yellow-300 transition">Experts</Link></li>
            </ul>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-yellow-400 mb-4">Get Started</h3>
            <ul className="space-y-2 text-sm">
              <li><Link to="/register" className="hover:text-yellow-300 transition">Create Account</Link></li>
              <li><Link to="/login" className="hover:text-yellow-300 transition">Sign In</Link></li>
              <li><Link to="/consultations/new" className="hover:text-yellow-300 transition">Request Consultation</Link></li>
            </ul>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-yellow-400 mb-4">Contact</h3>
            <ul className="space-y-2 text-sm">
              <li>Email: olesiaproskurnyak@gmail.com</li>
              <li>Phone: +38 (050) 770 87 60</li>
              <li>Address: Ukraine, Chernivtsi</li>
            </ul>
          </div>
        </div>
        <div className="border-t border-gray-700 mt-8 pt-6 text-center text-sm">
          <p>&copy; {new Date().getFullYear()} Askify. All rights reserved.</p>
        </div>
      </div>
    </footer>
  );
};

export default Footer;
