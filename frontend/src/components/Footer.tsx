import React from 'react';

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
              <li><a href="/" className="hover:text-yellow-300 transition">Home</a></li>
              <li><a href="/about" className="hover:text-yellow-300 transition">About Us</a></li>
              <li><a href="/consultations" className="hover:text-yellow-300 transition">Consultations</a></li>
              <li><a href="/experts" className="hover:text-yellow-300 transition">Experts</a></li>
            </ul>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-yellow-400 mb-4">Resources</h3>
            <ul className="space-y-2 text-sm">
              <li><a href="/faq" className="hover:text-yellow-300 transition">FAQ</a></li>
              <li><a href="/blog" className="hover:text-yellow-300 transition">Blog</a></li>
              <li><a href="/terms" className="hover:text-yellow-300 transition">Terms of Service</a></li>
              <li><a href="/privacy" className="hover:text-yellow-300 transition">Privacy Policy</a></li>
            </ul>
          </div>
          <div>
            <h3 className="text-lg font-semibold text-yellow-400 mb-4">Contact</h3>
            <ul className="space-y-2 text-sm">
              <li>Email: info@askify.com</li>
              <li>Phone: +1 (555) 123-4567</li>
              <li>Address: 123 Knowledge St, Wisdom City</li>
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
